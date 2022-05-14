using System;
using System.Numerics;
using DirectDimensional.Core;
using DirectDimensional.Core.Utilities;
using DirectDimensional.Bindings.Direct3D11;

using DDTexture2D = DirectDimensional.Core.Texture2D;
using static StbTrueTypeSharp.StbTrueType;

namespace DirectDimensional.Editor.GUI {
    /// <summary>
    /// Responsible for rendering widget interface. Do NOT handle widget clipping when outside scissor rect.
    /// </summary>
    public static unsafe class ImGuiRender {
        public static readonly int MaximumCircleSegment = 32;

        public static void DrawPoint(Vector2 point, Color32 color) {
            if (ImGui.CurrentWindow == null || color.A == 0) return;

            ImGuiContext.Vertices.Add(new(point, color));
            ImGuiContext.Indices.Add((ushort)(ImGuiContext.Vertices.Count - 1));

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = 1,
                IndexLocation = (ushort)(ImGuiContext.Vertices.Count - 1),

                ScissorsRect = ImGuiLowLevel.CurrentScissorRect,
                Topology = D3D11_PRIMITIVE_TOPOLOGY.PointList,
            });
        }
        public static void DrawPoints(ReadOnlySpan<Vector2> points, Color32 color) {
            if (ImGui.CurrentWindow == null || color.A == 0 || points.IsEmpty) return;

            for (int i = 0; i < points.Length; i++) {
                ImGuiContext.Vertices.Add(new(points[i], color));
                ImGuiContext.Indices.Add((ushort)(ImGuiContext.Vertices.Count - 1));
            }

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = (uint)points.Length,
                IndexLocation = (ushort)(ImGuiContext.Vertices.Count - points.Length),

                ScissorsRect = ImGuiLowLevel.CurrentScissorRect,
                Topology = D3D11_PRIMITIVE_TOPOLOGY.PointList,
            });
        }
        public static void DrawRect(Rect rect, Color32 color) {
            if (ImGui.CurrentWindow == null || color.A == 0 || rect.HasInvalidSize) return;

            var min = rect.Position + ImGuiLowLevel.CurrentCoordinateOffset;
            var max = rect.Max + ImGuiLowLevel.CurrentCoordinateOffset;

            var vcount = ImGuiContext.Vertices.Count;
            var icount = ImGuiContext.Indices.Count;

            ImGuiContext.Vertices.Add(new Vertex(min, color));
            ImGuiContext.Vertices.Add(new Vertex(new Vector2(max.X, min.Y), color));
            ImGuiContext.Vertices.Add(new Vertex(max, color));
            ImGuiContext.Vertices.Add(new Vertex(new Vector2(min.X, max.Y), color));

            ImGuiContext.Indices.Add((ushort)(vcount + 3));
            ImGuiContext.Indices.Add((ushort)vcount);
            ImGuiContext.Indices.Add((ushort)(vcount + 2));
            ImGuiContext.Indices.Add((ushort)(vcount + 1));

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = 4,
                IndexLocation = (ushort)icount,

                ScissorsRect = ImGuiLowLevel.CurrentScissorRect,
                Topology = D3D11_PRIMITIVE_TOPOLOGY.TriangleStrip
            });
        }
        public static void DrawCircle(Circle circle, Color32 color, int numSegments) {
            if (ImGui.CurrentWindow == null || circle.Radius < 1 || color.A == 0) return;

            numSegments = Math.Clamp(numSegments, 3, MaximumCircleSegment);

            var vcount = ImGuiContext.Vertices.Count;
            var icount = ImGuiContext.Indices.Count;

            circle.Center += ImGuiLowLevel.CurrentCoordinateOffset;

            float angleStep = MathF.PI * 2 / numSegments;
            for (int i = 0; i < numSegments; i++) {
                (float sin, float cos) = MathF.SinCos(angleStep * i);
                ImGuiContext.Vertices.Add(new(new Vector2(cos, -sin) * circle.Radius + circle.Center, color));
            }

            for (int i = 0; i < numSegments - 2; i++) {
                ImGuiContext.Indices.Add((ushort)vcount);
                ImGuiContext.Indices.Add((ushort)(vcount + i + 1));
                ImGuiContext.Indices.Add((ushort)(vcount + i + 2));
            }

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = (ushort)(ImGuiContext.Indices.Count - icount),
                IndexLocation = (ushort)icount,

                ScissorsRect = ImGuiLowLevel.CurrentScissorRect,
                Topology = D3D11_PRIMITIVE_TOPOLOGY.TriangleStrip
            });
        }
        public static void DrawCircle(Circle circle, Color32 color, int numSegments, float start, float normalize) {
            if (ImGui.CurrentWindow == null || circle.Radius < 1 || color.A == 0) return;
            if (normalize == 1) {
                DrawCircle(circle, color, numSegments);
                return;
            }

            numSegments = Math.Clamp(numSegments, 3, MaximumCircleSegment);

            var vcount = ImGuiContext.Vertices.Count;
            var icount = ImGuiContext.Indices.Count;

            circle.Center += ImGuiLowLevel.CurrentCoordinateOffset;

            float angleStep = MathF.PI * 2 * normalize / numSegments;
            numSegments += 1;

            ImGuiContext.Vertices.Add(new(circle.Center, color));

            for (int i = 0; i < numSegments; i++) {
                (float sin, float cos) = MathF.SinCos(start + angleStep * i);
                ImGuiContext.Vertices.Add(new(new Vector2(cos, -sin) * circle.Radius + circle.Center, color));
            }

            for (int i = 0; i < numSegments - 1; i++) {
                ImGuiContext.Indices.Add((ushort)vcount);
                ImGuiContext.Indices.Add((ushort)(vcount + i + 1));
                ImGuiContext.Indices.Add((ushort)(vcount + i + 2));
            }

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = (ushort)(ImGuiContext.Indices.Count - icount),
                IndexLocation = (ushort)icount,

                ScissorsRect = ImGuiLowLevel.CurrentScissorRect,
                Topology = D3D11_PRIMITIVE_TOPOLOGY.TriangleList,
            });
        }
        public static void DrawLine(Line line, Color32 color) {
            if (ImGui.CurrentWindow == null || color.A == 0) return;

            var vcount = ImGuiContext.Vertices.Count;
            var icount = ImGuiContext.Indices.Count;

            ImGuiContext.Vertices.Add(new(line.Start + ImGuiLowLevel.CurrentCoordinateOffset, color));
            ImGuiContext.Vertices.Add(new(line.End + ImGuiLowLevel.CurrentCoordinateOffset, color));

            ImGuiContext.Indices.Add((ushort)vcount);
            ImGuiContext.Indices.Add((ushort)(vcount + 1));

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = 2,
                IndexLocation = (ushort)icount,

                ScissorsRect = ImGuiLowLevel.CurrentScissorRect,
                Topology = D3D11_PRIMITIVE_TOPOLOGY.LineList,
            });
        }
        public static void DrawLine(Line line, float thickness, Color32 color) {
            if (ImGui.CurrentWindow == null || color.A == 0 || thickness == 0) return;
            if (-1 <= thickness && thickness <= 1) {
                DrawLine(line, color);
                return;
            }

            var vcount = ImGuiContext.Vertices.Count;
            var icount = ImGuiContext.Indices.Count;

            var cwnorm = Vector2.Normalize(line.CWNormal) * thickness / 2;
            var ccwnorm = Vector2.Normalize(line.CCWNormal) * thickness / 2;

            ImGuiContext.Vertices.Add(new(line.Start + cwnorm + ImGuiLowLevel.CurrentCoordinateOffset, color));
            ImGuiContext.Vertices.Add(new(line.End + cwnorm + ImGuiLowLevel.CurrentCoordinateOffset, color));
            ImGuiContext.Vertices.Add(new(line.End + ccwnorm + ImGuiLowLevel.CurrentCoordinateOffset, color));
            ImGuiContext.Vertices.Add(new(line.Start + ccwnorm + ImGuiLowLevel.CurrentCoordinateOffset, color));

            ImGuiContext.Indices.Add((ushort)vcount);
            ImGuiContext.Indices.Add((ushort)(vcount + 1));
            ImGuiContext.Indices.Add((ushort)(vcount + 2));
            ImGuiContext.Indices.Add((ushort)(vcount + 2));
            ImGuiContext.Indices.Add((ushort)(vcount + 3));
            ImGuiContext.Indices.Add((ushort)vcount);

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = 6,
                IndexLocation = (uint)icount,

                ScissorsRect = ImGuiLowLevel.CurrentScissorRect,
            });
        }
        public static void DrawLineList(ReadOnlySpan<Line> lines, Color32 color) {
            if (ImGui.CurrentWindow == null || color.A == 0 || lines.Length == 0) return;

            var icount = ImGuiContext.Indices.Count;

            for (int i = 0; i < lines.Length; i++) {
                var vcount = ImGuiContext.Vertices.Count;

                ImGuiContext.Vertices.Add(new(lines[i].Start + ImGuiLowLevel.CurrentCoordinateOffset, color));
                ImGuiContext.Vertices.Add(new(lines[i].End + ImGuiLowLevel.CurrentCoordinateOffset, color));

                ImGuiContext.Indices.Add((ushort)vcount);
                ImGuiContext.Indices.Add((ushort)(vcount + 1));
            }

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = (uint)(lines.Length * 2),
                IndexLocation = (ushort)icount,

                ScissorsRect = ImGuiLowLevel.CurrentScissorRect,
                Topology = D3D11_PRIMITIVE_TOPOLOGY.LineList,
            });
        }
        public static void DrawLineStrip(ReadOnlySpan<Vector2> points, Color32 color) {
            if (ImGui.CurrentWindow == null || color.A == 0 || points.Length == 0) return;

            var icount = ImGuiContext.Indices.Count;

            for (int i = 0; i < points.Length; i++) {
                ImGuiContext.Vertices.Add(new(points[i] + ImGuiLowLevel.CurrentCoordinateOffset, color));
                ImGuiContext.Indices.Add((ushort)(ImGuiContext.Vertices.Count - 1));
            }

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = (uint)points.Length,
                IndexLocation = (ushort)icount,

                ScissorsRect = ImGuiLowLevel.CurrentScissorRect,
                Topology = D3D11_PRIMITIVE_TOPOLOGY.LineStrip,
            });
        }
        public static void DrawFrame(Rect rect, Color32 color) {
            if (ImGui.CurrentWindow == null || color.A == 0 || rect.HasInvalidSize) return;

            Vector2 min = rect.Position + ImGuiLowLevel.CurrentCoordinateOffset + new Vector2(0.5f), max = rect.Max + ImGuiLowLevel.CurrentCoordinateOffset - new Vector2(0.49f);

            var vcount = ImGuiContext.Vertices.Count;
            var icount = ImGuiContext.Indices.Count;

            ImGuiContext.Vertices.Add(new Vertex(min, color));
            ImGuiContext.Vertices.Add(new Vertex(new Vector2(max.X, min.Y), color));
            ImGuiContext.Vertices.Add(new Vertex(max, color));
            ImGuiContext.Vertices.Add(new Vertex(new Vector2(min.X, max.Y), color));

            ImGuiContext.Indices.Add((ushort)vcount);
            ImGuiContext.Indices.Add((ushort)(vcount + 1));
            ImGuiContext.Indices.Add((ushort)(vcount + 2));
            ImGuiContext.Indices.Add((ushort)(vcount + 3));
            ImGuiContext.Indices.Add((ushort)vcount);

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = 5,
                IndexLocation = (ushort)icount,

                ScissorsRect = ImGuiLowLevel.CurrentScissorRect,
                Topology = D3D11_PRIMITIVE_TOPOLOGY.LineStrip,
            });
        }
        public static void DrawFrame(Rect rect, float thickness, Color32 color) {
            if (ImGui.CurrentWindow == null || color.A == 0 || rect.HasInvalidSize) return;

            if (thickness >= rect.Width / 2 || thickness >= rect.Height / 2) {
                DrawRect(rect, color);
                return;
            }
            if (thickness <= 1) {
                DrawFrame(rect, color);
                return;
            }

            var vcount = ImGuiContext.Vertices.Count;
            var icount = ImGuiContext.Indices.Count;

            // Outer vertices
            Vector2 min = rect.Position + ImGuiLowLevel.CurrentCoordinateOffset + new Vector2(0.5f), max = rect.Max + ImGuiLowLevel.CurrentCoordinateOffset - new Vector2(0.49f);
            ImGuiContext.Vertices.Add(new Vertex(min, color));
            ImGuiContext.Vertices.Add(new Vertex(new Vector2(max.X, min.Y), color));
            ImGuiContext.Vertices.Add(new Vertex(max, color));
            ImGuiContext.Vertices.Add(new Vertex(new Vector2(min.X, max.Y), color));
            // Inner vertices
            rect.Extrude(-thickness);
            min = rect.Position + ImGuiLowLevel.CurrentCoordinateOffset + new Vector2(0.5f);
            max = rect.Max + ImGuiLowLevel.CurrentCoordinateOffset - new Vector2(0.49f);

            ImGuiContext.Vertices.Add(new Vertex(min, color));
            ImGuiContext.Vertices.Add(new Vertex(new Vector2(max.X, min.Y), color));
            ImGuiContext.Vertices.Add(new Vertex(max, color));
            ImGuiContext.Vertices.Add(new Vertex(new Vector2(min.X, max.Y), color));

            ImGuiContext.Indices.Add((ushort)vcount);
            ImGuiContext.Indices.Add((ushort)(vcount + 4));
            ImGuiContext.Indices.Add((ushort)(vcount + 1));
            ImGuiContext.Indices.Add((ushort)(vcount + 5));
            ImGuiContext.Indices.Add((ushort)(vcount + 2));
            ImGuiContext.Indices.Add((ushort)(vcount + 6));
            ImGuiContext.Indices.Add((ushort)(vcount + 3));
            ImGuiContext.Indices.Add((ushort)(vcount + 7));
            ImGuiContext.Indices.Add((ushort)vcount);
            ImGuiContext.Indices.Add((ushort)(vcount + 4));

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = 10,
                IndexLocation = (ushort)icount,

                ScissorsRect = ImGuiLowLevel.CurrentScissorRect,
                Topology = D3D11_PRIMITIVE_TOPOLOGY.TriangleStrip,
            });
        }
        public static void DrawOutlinedCircle(Circle circle, Color32 color, int numSegments, float thickness) {
            if (ImGui.CurrentWindow == null || circle.Radius < 1 || color.A == 0 || thickness <= 0.5f) return;

            if (thickness >= circle.Radius) {
                DrawCircle(circle, color, numSegments);
                return;
            }

            numSegments = Math.Clamp(numSegments, 3, MaximumCircleSegment);

            var vcount = ImGuiContext.Vertices.Count;
            var icount = ImGuiContext.Indices.Count;

            circle.Center += ImGuiLowLevel.CurrentCoordinateOffset;

            float angleStep = MathF.PI * 2 / numSegments;

            for (int i = 0; i < numSegments; i++) {
                (float sin, float cos) = MathF.SinCos(angleStep * i);

                ImGuiContext.Vertices.Add(new(new Vector2(cos, -sin) * circle.Radius + circle.Center, color));
                ImGuiContext.Vertices.Add(new(new Vector2(cos, -sin) * (circle.Radius - thickness) + circle.Center, color));
            }

            ImGuiContext.Indices.Add((ushort)vcount);
            ImGuiContext.Indices.Add((ushort)(vcount + 1));
            ImGuiContext.Indices.Add((ushort)(vcount + 2));

            for (int i = 3; i < numSegments * 2; i++) {
                ImGuiContext.Indices.Add((ushort)(vcount + i));
            }

            ImGuiContext.Indices.Add((ushort)vcount);
            ImGuiContext.Indices.Add((ushort)(vcount + 1));

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = (ushort)(ImGuiContext.Indices.Count - icount),
                IndexLocation = (ushort)icount,

                ScissorsRect = ImGuiLowLevel.CurrentScissorRect,
                Topology = D3D11_PRIMITIVE_TOPOLOGY.TriangleStrip,
            });
        }
        public static void DrawOutlinedCircle(Circle circle, Color32 color, int numSegments, float thickness, float start, float normalize) {
            normalize = DDMath.Saturate(normalize);
            if (ImGui.CurrentWindow == null || circle.Radius < 1 || color.A == 0 || thickness <= 0.5f || normalize == 0) return;

            if (thickness >= circle.Radius) {
                DrawCircle(circle, color, numSegments);
                return;
            }

            numSegments = Math.Clamp(numSegments, 3, MaximumCircleSegment);

            var vcount = ImGuiContext.Vertices.Count;
            var icount = ImGuiContext.Indices.Count;

            circle.Center += ImGuiLowLevel.CurrentCoordinateOffset;

            float angleStep = MathF.PI * 2 * normalize / numSegments;
            numSegments += 1;

            for (int i = 0; i < numSegments; i++) {
                (float sin, float cos) = MathF.SinCos(start + angleStep * i);

                ImGuiContext.Vertices.Add(new(new Vector2(cos, -sin) * circle.Radius + circle.Center, color));
                ImGuiContext.Vertices.Add(new(new Vector2(cos, -sin) * (circle.Radius - thickness) + circle.Center, color));
            }

            ImGuiContext.Indices.Add((ushort)vcount);
            ImGuiContext.Indices.Add((ushort)(vcount + 1));
            ImGuiContext.Indices.Add((ushort)(vcount + 2));

            for (int i = 3; i < numSegments * 2; i++) {
                ImGuiContext.Indices.Add((ushort)(vcount + i));
            }

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = (ushort)(ImGuiContext.Indices.Count - icount),
                IndexLocation = (ushort)icount,

                ScissorsRect = ImGuiLowLevel.CurrentScissorRect,
                Topology = D3D11_PRIMITIVE_TOPOLOGY.TriangleStrip,
            });
        }
        public static void DrawTexturedRect(Vertex topLeft, Vertex topRight, Vertex bottomRight, Vertex bottomLeft, DDTexture2D texture) {
            if (!texture.IsRenderable) {
                Logger.Warn("Texture used for rendering isn't flagged as Renderable");
                return;
            }

            ImGuiLowLevel.DrawTexturedRect(topLeft, topRight, bottomRight, bottomLeft, texture.DXSRV, texture.DXSampler);
        }

        public static void DrawGradientRect(Rect rect, Gradient gradient) {
            if (ImGui.CurrentWindow == null || rect.HasInvalidSize) return;

            var keys = gradient.Keys;
            switch (keys.Count) {
                case 0:
                    DrawRect(rect, Gradient.FallbackColor);
                    break;

                case 1: {
                    DrawRect(rect, keys[0].Color);
                    break;
                }

                default: {
                    int beginKey = 0;
                    float segmentWidth;

                    int vcount;
                    var icount = ImGuiContext.Indices.Count;

                    var rectMax = rect.Max;

                    if (keys[0].Position != 0) {
                        switch (keys[0].Mode) {
                            default:
                                segmentWidth = rect.Width * keys[0].NormalizedPosition;

                                vcount = ImGuiContext.Vertices.Count;

                                ImGuiContext.Vertices.Add(new(rect.Position + ImGuiLowLevel.CurrentCoordinateOffset, keys[0].Color));
                                ImGuiContext.Vertices.Add(new(new Vector2(rect.X + segmentWidth, rect.Y) + ImGuiLowLevel.CurrentCoordinateOffset, keys[0].Color));
                                ImGuiContext.Vertices.Add(new(new Vector2(rect.X + segmentWidth, rectMax.Y) + ImGuiLowLevel.CurrentCoordinateOffset, keys[0].Color));
                                ImGuiContext.Vertices.Add(new(new Vector2(rect.X, rectMax.Y) + ImGuiLowLevel.CurrentCoordinateOffset, keys[0].Color));

                                ImGuiContext.Indices.Add((ushort)(vcount + 3));
                                ImGuiContext.Indices.Add((ushort)vcount);
                                ImGuiContext.Indices.Add((ushort)(vcount + 2));
                                ImGuiContext.Indices.Add((ushort)(vcount + 1));
                                break;

                            case GradientColorMode.Fixed: {
                                segmentWidth = rect.Width * keys[1].NormalizedPosition;

                                vcount = ImGuiContext.Vertices.Count;

                                ImGuiContext.Vertices.Add(new(rect.Position + ImGuiLowLevel.CurrentCoordinateOffset, keys[0].Color));
                                ImGuiContext.Vertices.Add(new(new Vector2(rect.X + segmentWidth, rect.Y) + ImGuiLowLevel.CurrentCoordinateOffset, keys[0].Color));
                                ImGuiContext.Vertices.Add(new(new Vector2(rect.X + segmentWidth, rectMax.Y) + ImGuiLowLevel.CurrentCoordinateOffset, keys[0].Color));
                                ImGuiContext.Vertices.Add(new(new Vector2(rect.X, rectMax.Y) + ImGuiLowLevel.CurrentCoordinateOffset, keys[0].Color));

                                ImGuiContext.Indices.Add((ushort)(vcount + 3));
                                ImGuiContext.Indices.Add((ushort)vcount);
                                ImGuiContext.Indices.Add((ushort)(vcount + 2));
                                ImGuiContext.Indices.Add((ushort)(vcount + 1));

                                beginKey = 1;
                                break;
                            }
                        }
                        //float norm = list[0].Position / ushort.MaxValue;

                        //x = rect.Width * norm;
                        //var segmentSize = rect.Width * norm;

                        //vcount = ImGuiContext.Vertices.Count;

                        //ImGuiContext.Vertices.Add(new(rect.Position + ImGuiLowLevel.CoordinateOffset, Gradient.FallbackColor));
                        //ImGuiContext.Vertices.Add(new(new Vector2(segmentSize, 260) + ImGuiLowLevel.CoordinateOffset, Gradient.FallbackColor));
                        //ImGuiContext.Vertices.Add(new(new Vector2(segmentSize, 290) + ImGuiLowLevel.CoordinateOffset, Gradient.FallbackColor));
                        //ImGuiContext.Vertices.Add(new(new Vector2(0, 290) + ImGuiLowLevel.CoordinateOffset, Gradient.FallbackColor));

                        //ImGuiContext.Indices.Add((ushort)(vcount + 3));
                        //ImGuiContext.Indices.Add((ushort)vcount);
                        //ImGuiContext.Indices.Add((ushort)(vcount + 2));
                        //ImGuiContext.Indices.Add((ushort)(vcount + 1));

                        //x += segmentSize;
                    }

                    float xPos = 0;
                    for (int i = beginKey; i < keys.Count - 1; i++) {
                        var disp = keys[i + 1].Position - keys[i].Position;

                        float segmentSize = rect.Width * ((float)disp / ushort.MaxValue);

                        vcount = ImGuiContext.Vertices.Count;

                        ImGuiContext.Vertices.Add(new Vertex(new Vector2(xPos, rect.Y) + ImGuiLowLevel.CurrentCoordinateOffset, keys[i].Color));
                        ImGuiContext.Vertices.Add(new Vertex(new Vector2(xPos, rectMax.Y) + ImGuiLowLevel.CurrentCoordinateOffset, keys[i].Color));

                        ImGuiContext.Indices.Add((ushort)(vcount + 1));
                        ImGuiContext.Indices.Add((ushort)vcount);

                        xPos += segmentSize;

                        if (keys[i].Mode == GradientColorMode.Fixed) {
                            ImGuiContext.Vertices.Add(new Vertex(new Vector2(xPos, rect.Y) + ImGuiLowLevel.CurrentCoordinateOffset, keys[i].Color));
                            ImGuiContext.Vertices.Add(new Vertex(new Vector2(xPos, rectMax.Y) + ImGuiLowLevel.CurrentCoordinateOffset, keys[i].Color));

                            ImGuiContext.Indices.Add((ushort)(vcount + 3));
                            ImGuiContext.Indices.Add((ushort)(vcount + 2));
                        }
                    }

                    vcount = ImGuiContext.Vertices.Count;

                    ImGuiContext.Vertices.Add(new Vertex(new Vector2(xPos, rect.Y) + ImGuiLowLevel.CurrentCoordinateOffset, keys[^1].Color));
                    ImGuiContext.Vertices.Add(new Vertex(new Vector2(xPos, rectMax.Y) + ImGuiLowLevel.CurrentCoordinateOffset, keys[^1].Color));

                    ImGuiContext.Vertices.Add(new Vertex(new Vector2(rect.Width, rect.Y) + ImGuiLowLevel.CurrentCoordinateOffset, keys[^1].Color));
                    ImGuiContext.Vertices.Add(new Vertex(new Vector2(rect.Width, rectMax.Y) + ImGuiLowLevel.CurrentCoordinateOffset, keys[^1].Color));

                    ImGuiContext.Indices.Add((ushort)(vcount + 1));
                    ImGuiContext.Indices.Add((ushort)vcount);
                    ImGuiContext.Indices.Add((ushort)(vcount + 3));
                    ImGuiContext.Indices.Add((ushort)(vcount + 2));

                    ImGui.CurrentWindow.DrawCalls.Add(new DrawCall() {
                        IndexCount = (uint)(ImGuiContext.Indices.Count - icount),
                        IndexLocation = (uint)icount,

                        ScissorsRect = ImGuiLowLevel.CurrentScissorRect,
                        Topology = D3D11_PRIMITIVE_TOPOLOGY.TriangleStrip,
                    });

                    break;
                }
            }
        }

        /// <summary>
        /// Requires Triangle List topology
        /// </summary>
        public static void AddCompositeRect(Rect rect, Color32 color) {
            if (ImGui.CurrentWindow == null || !ImGui.CurrentWindow.IsDrawingComposite || color.A == 0 || rect.HasInvalidSize) return;

            var vcount = ImGuiContext.Vertices.Count;

            var min = rect.Position + ImGuiLowLevel.CurrentCoordinateOffset;
            var max = rect.Max + ImGuiLowLevel.CurrentCoordinateOffset;

            ImGuiLowLevel.AddVertices(new Vertex(min, color));
            ImGuiLowLevel.AddVertices(new Vertex(new Vector2(max.X, min.Y), color));
            ImGuiLowLevel.AddVertices(new Vertex(max, color));
            ImGuiLowLevel.AddVertices(new Vertex(new Vector2(min.X, max.Y), color));

            ImGuiLowLevel.AddIndices(vcount);
            ImGuiLowLevel.AddIndices(vcount + 1);
            ImGuiLowLevel.AddIndices(vcount + 2);
            ImGuiLowLevel.AddIndices(vcount + 2);
            ImGuiLowLevel.AddIndices(vcount + 3);
            ImGuiLowLevel.AddIndices(vcount);
        }

        /// <summary>
        /// Requires Triangle List topology
        /// </summary>
        public static void AddCompositeCircle(Circle circle, Color32 color, int numSegments) {
            if (ImGui.CurrentWindow == null || circle.Radius < 1 || circle.Radius < 1) return;

            numSegments = Math.Clamp(numSegments, 3, MaximumCircleSegment);

            var vcount = ImGuiContext.Vertices.Count;

            circle.Center += ImGuiLowLevel.CurrentCoordinateOffset;

            float angleStep = MathF.PI * 2 / numSegments;
            for (int i = 0; i < numSegments; i++) {
                (float sin, float cos) = MathF.SinCos(angleStep * i);
                ImGuiContext.Vertices.Add(new(new Vector2(cos, -sin) * circle.Radius + circle.Center, color));
            }

            for (int i = 0; i < numSegments - 2; i++) {
                ImGuiContext.Indices.Add((ushort)vcount);
                ImGuiContext.Indices.Add((ushort)(vcount + i + 1));
                ImGuiContext.Indices.Add((ushort)(vcount + i + 2));
            }
        }

        /// <summary>
        /// Requires Triangle List topology
        /// </summary>
        public static void AddCompositeOutlinedCircle(Circle circle, float thickness, Color32 color, int numSegments) {
            if (ImGui.CurrentWindow == null || circle.Radius < 1 || color.A == 0 || thickness <= 0.5f) return;

            if (thickness >= circle.Radius) {
                AddCompositeCircle(circle, color, numSegments);
                return;
            }

            numSegments = Math.Clamp(numSegments, 3, MaximumCircleSegment);

            var vcount = ImGuiContext.Vertices.Count;

            circle.Center += ImGuiLowLevel.CurrentCoordinateOffset;

            float angleStep = MathF.PI * 2 / numSegments;

            for (int i = 0; i < numSegments; i++) {
                (float sin, float cos) = MathF.SinCos(angleStep * i);

                ImGuiContext.Vertices.Add(new(new Vector2(cos, -sin) * circle.Radius + circle.Center, color));
                ImGuiContext.Vertices.Add(new(new Vector2(cos, -sin) * (circle.Radius - thickness) + circle.Center, color));
            }

            for (int i = 0; i < numSegments - 1; i++) {
                int outsideIndex = vcount + i * 2;

                ImGuiContext.Indices.Add((ushort)outsideIndex);
                ImGuiContext.Indices.Add((ushort)(outsideIndex + 1));
                ImGuiContext.Indices.Add((ushort)(outsideIndex + 2));
                ImGuiContext.Indices.Add((ushort)(outsideIndex + 2));
                ImGuiContext.Indices.Add((ushort)(outsideIndex + 3));
                ImGuiContext.Indices.Add((ushort)(outsideIndex + 1));
            }

            int last = vcount + (numSegments - 1) * 2;
            ImGuiContext.Indices.Add((ushort)last);
            ImGuiContext.Indices.Add((ushort)(last + 1));
            ImGuiContext.Indices.Add((ushort)vcount);
            ImGuiContext.Indices.Add((ushort)vcount);
            ImGuiContext.Indices.Add((ushort)(vcount + 1));
            ImGuiContext.Indices.Add((ushort)(last + 1));
        }

        /// <summary>
        /// Requires Line List topology
        /// </summary>
        public static void AddCompositeLine(Line line, Color32 color) {
            if (ImGui.CurrentWindow == null || !ImGui.CurrentWindow.IsDrawingComposite || color.A == 0) return;

            var vcount = ImGuiContext.Vertices.Count;

            ImGuiContext.Vertices.Add(new(line.Start + ImGuiLowLevel.CurrentCoordinateOffset, color));
            ImGuiContext.Vertices.Add(new(line.End + ImGuiLowLevel.CurrentCoordinateOffset, color));

            ImGuiContext.Indices.Add((ushort)vcount);
            ImGuiContext.Indices.Add((ushort)(vcount + 1));
        }

        /// <summary>
        /// Requires Line List topology
        /// </summary>
        public static void AddCompositeLineList(ReadOnlySpan<Line> lines, Color32 color) {
            if (ImGui.CurrentWindow == null || !ImGui.CurrentWindow.IsDrawingComposite || color.A == 0 || lines.Length == 0) return;

            for (int i = 0; i < lines.Length; i++) {
                var vcount = ImGuiContext.Vertices.Count;

                ImGuiContext.Vertices.Add(new(lines[i].Start + ImGuiLowLevel.CurrentCoordinateOffset, color));
                ImGuiContext.Vertices.Add(new(lines[i].End + ImGuiLowLevel.CurrentCoordinateOffset, color));

                ImGuiContext.Indices.Add((ushort)vcount);
                ImGuiContext.Indices.Add((ushort)(vcount + 1));
            }
        }

        /// <summary>
        /// Requires Line Strip topology
        /// </summary>
        public static void AddCompositeLineStrip(ReadOnlySpan<Vector2> points, Color32 color) {
            if (ImGui.CurrentWindow == null || !ImGui.CurrentWindow.IsDrawingComposite || color.A == 0 || points.Length == 0) return;

            for (int i = 0; i < points.Length; i++) {
                ImGuiContext.Vertices.Add(new(points[i] + ImGuiLowLevel.CurrentCoordinateOffset, color));
                ImGuiContext.Indices.Add((ushort)(ImGuiContext.Vertices.Count - 1));
            }
        }

        /// <summary>
        /// Draw a character at given position. Bottom left anchor
        /// </summary>
        public static void AddCharacterQuad_Unchecked(float xPosition, float yPosition, char character, Vector2 uvStep, Color32 color) {
            fixed (stbtt_packedchar* ptr = &EditorResources.PackedChar[0]) {
                AddCharacterQuad_Raw(xPosition, yPosition, ptr + character, uvStep, color);
            }
        }

        /// <summary>
        /// Draw a character at given position. Bottom left anchor
        /// </summary>
        public static void AddCharacterQuad_Raw(float xPosition, float yPosition, stbtt_packedchar* pCharacterQuad, Vector2 uvStep, Color32 color) {
            float minX = MathF.Floor(xPosition + pCharacterQuad->xoff + 0.5f);
            float minY = MathF.Floor(yPosition + pCharacterQuad->yoff + 0.5f);

            float maxX = (int)(minX + pCharacterQuad->xoff2 - pCharacterQuad->xoff);
            float maxY = (int)(minY + pCharacterQuad->yoff2 - pCharacterQuad->yoff);

            Vector2 minUV = new Vector2(pCharacterQuad->x0, pCharacterQuad->y0) * uvStep;
            Vector2 maxUV = new Vector2(pCharacterQuad->x1, pCharacterQuad->y1) * uvStep;

            var vcount = ImGuiContext.Vertices.Count;

            ImGuiLowLevel.AddVertices(new Vertex(new(minX, minY), color, minUV));
            ImGuiLowLevel.AddVertices(new Vertex(new(maxX, minY), color, new Vector2(maxUV.X, minUV.Y)));
            ImGuiLowLevel.AddVertices(new Vertex(new(maxX, maxY), color, maxUV));
            ImGuiLowLevel.AddVertices(new Vertex(new(minX, maxY), color, new Vector2(minUV.X, maxUV.Y)));

            ImGuiLowLevel.AddIndex(vcount);
            ImGuiLowLevel.AddIndex(vcount + 1);
            ImGuiLowLevel.AddIndex(vcount + 2);
            ImGuiLowLevel.AddIndex(vcount + 2);
            ImGuiLowLevel.AddIndex(vcount + 3);
            ImGuiLowLevel.AddIndex(vcount);
        }
    }
}
