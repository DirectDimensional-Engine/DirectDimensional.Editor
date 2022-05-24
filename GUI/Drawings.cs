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
    public static unsafe class Drawings {
        public static readonly int MaximumCircleSegment = 32;

        public static void DrawPoint(Vector2 point, Color32 color) {
            if (ImGui.CurrentWindow == null || color.A == 0) return;

            Context.Vertices.Add(new(point, color));
            Context.Indices.Add((ushort)(Context.Vertices.Count - 1));

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = 1,
                IndexLocation = (ushort)(Context.Vertices.Count - 1),

                ScissorsRect = LowLevel.CurrentScissorRect,
                Topology = D3D11_PRIMITIVE_TOPOLOGY.PointList,
            });
        }
        public static void DrawPoints(ReadOnlySpan<Vector2> points, Color32 color) {
            if (ImGui.CurrentWindow == null || color.A == 0 || points.IsEmpty) return;

            for (int i = 0; i < points.Length; i++) {
                Context.Vertices.Add(new(points[i], color));
                Context.Indices.Add((ushort)(Context.Vertices.Count - 1));
            }

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = (uint)points.Length,
                IndexLocation = (ushort)(Context.Vertices.Count - points.Length),

                ScissorsRect = LowLevel.CurrentScissorRect,
                Topology = D3D11_PRIMITIVE_TOPOLOGY.PointList,
            });
        }
        public static void DrawRect(Rect rect, Color32 color) {
            if (ImGui.CurrentWindow == null || color.A == 0 || rect.HasInvalidSize) return;

            var min = rect.Position + LowLevel.CurrentCoordinateOffset;
            var max = rect.Max + LowLevel.CurrentCoordinateOffset;

            var vcount = Context.Vertices.Count;
            var icount = Context.Indices.Count;

            Context.Vertices.Add(new Vertex(min, color));
            Context.Vertices.Add(new Vertex(new Vector2(max.X, min.Y), color));
            Context.Vertices.Add(new Vertex(max, color));
            Context.Vertices.Add(new Vertex(new Vector2(min.X, max.Y), color));

            Context.Indices.Add((ushort)(vcount + 3));
            Context.Indices.Add((ushort)vcount);
            Context.Indices.Add((ushort)(vcount + 2));
            Context.Indices.Add((ushort)(vcount + 1));

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = 4,
                IndexLocation = (ushort)icount,

                ScissorsRect = LowLevel.CurrentScissorRect,
                Topology = D3D11_PRIMITIVE_TOPOLOGY.TriangleStrip
            });
        }
        public static void DrawCircle(Circle circle, Color32 color, int numSegments) {
            if (ImGui.CurrentWindow == null || circle.Radius < 1 || color.A == 0) return;

            numSegments = Math.Clamp(numSegments, 3, MaximumCircleSegment);

            var vcount = Context.Vertices.Count;
            var icount = Context.Indices.Count;

            circle.Center += LowLevel.CurrentCoordinateOffset;

            float angleStep = MathF.PI * 2 / numSegments;
            for (int i = 0; i < numSegments; i++) {
                (float sin, float cos) = MathF.SinCos(angleStep * i);
                Context.Vertices.Add(new(new Vector2(cos, -sin) * circle.Radius + circle.Center, color));
            }

            for (int i = 0; i < numSegments - 2; i++) {
                Context.Indices.Add((ushort)vcount);
                Context.Indices.Add((ushort)(vcount + i + 1));
                Context.Indices.Add((ushort)(vcount + i + 2));
            }

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = (ushort)(Context.Indices.Count - icount),
                IndexLocation = (ushort)icount,

                ScissorsRect = LowLevel.CurrentScissorRect,
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

            var vcount = Context.Vertices.Count;
            var icount = Context.Indices.Count;

            circle.Center += LowLevel.CurrentCoordinateOffset;

            float angleStep = MathF.PI * 2 * normalize / numSegments;
            numSegments += 1;

            Context.Vertices.Add(new(circle.Center, color));

            for (int i = 0; i < numSegments; i++) {
                (float sin, float cos) = MathF.SinCos(start + angleStep * i);
                Context.Vertices.Add(new(new Vector2(cos, -sin) * circle.Radius + circle.Center, color));
            }

            for (int i = 0; i < numSegments - 1; i++) {
                Context.Indices.Add((ushort)vcount);
                Context.Indices.Add((ushort)(vcount + i + 1));
                Context.Indices.Add((ushort)(vcount + i + 2));
            }

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = (ushort)(Context.Indices.Count - icount),
                IndexLocation = (ushort)icount,

                ScissorsRect = LowLevel.CurrentScissorRect,
                Topology = D3D11_PRIMITIVE_TOPOLOGY.TriangleList,
            });
        }
        public static void DrawLine(Line line, Color32 color) {
            if (ImGui.CurrentWindow == null || color.A == 0) return;

            var vcount = Context.Vertices.Count;
            var icount = Context.Indices.Count;

            Context.Vertices.Add(new(line.Start + LowLevel.CurrentCoordinateOffset, color));
            Context.Vertices.Add(new(line.End + LowLevel.CurrentCoordinateOffset, color));

            Context.Indices.Add((ushort)vcount);
            Context.Indices.Add((ushort)(vcount + 1));

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = 2,
                IndexLocation = (ushort)icount,

                ScissorsRect = LowLevel.CurrentScissorRect,
                Topology = D3D11_PRIMITIVE_TOPOLOGY.LineList,
            });
        }
        public static void DrawLine(Line line, float thickness, Color32 color) {
            if (ImGui.CurrentWindow == null || color.A == 0 || thickness == 0) return;
            if (-1 <= thickness && thickness <= 1) {
                DrawLine(line, color);
                return;
            }

            var vcount = Context.Vertices.Count;
            var icount = Context.Indices.Count;

            var cwnorm = Vector2.Normalize(line.CWNormal) * thickness / 2;
            var ccwnorm = Vector2.Normalize(line.CCWNormal) * thickness / 2;

            Context.Vertices.Add(new(line.Start + cwnorm + LowLevel.CurrentCoordinateOffset, color));
            Context.Vertices.Add(new(line.End + cwnorm + LowLevel.CurrentCoordinateOffset, color));
            Context.Vertices.Add(new(line.End + ccwnorm + LowLevel.CurrentCoordinateOffset, color));
            Context.Vertices.Add(new(line.Start + ccwnorm + LowLevel.CurrentCoordinateOffset, color));

            Context.Indices.Add((ushort)vcount);
            Context.Indices.Add((ushort)(vcount + 1));
            Context.Indices.Add((ushort)(vcount + 2));
            Context.Indices.Add((ushort)(vcount + 2));
            Context.Indices.Add((ushort)(vcount + 3));
            Context.Indices.Add((ushort)vcount);

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = 6,
                IndexLocation = (uint)icount,

                ScissorsRect = LowLevel.CurrentScissorRect,
            });
        }
        public static void DrawLineList(ReadOnlySpan<Line> lines, Color32 color) {
            if (ImGui.CurrentWindow == null || color.A == 0 || lines.Length == 0) return;

            var icount = Context.Indices.Count;

            for (int i = 0; i < lines.Length; i++) {
                var vcount = Context.Vertices.Count;

                Context.Vertices.Add(new(lines[i].Start + LowLevel.CurrentCoordinateOffset, color));
                Context.Vertices.Add(new(lines[i].End + LowLevel.CurrentCoordinateOffset, color));

                Context.Indices.Add((ushort)vcount);
                Context.Indices.Add((ushort)(vcount + 1));
            }

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = (uint)(lines.Length * 2),
                IndexLocation = (ushort)icount,

                ScissorsRect = LowLevel.CurrentScissorRect,
                Topology = D3D11_PRIMITIVE_TOPOLOGY.LineList,
            });
        }
        public static void DrawLineStrip(ReadOnlySpan<Vector2> points, Color32 color) {
            if (ImGui.CurrentWindow == null || color.A == 0 || points.Length == 0) return;

            var icount = Context.Indices.Count;

            for (int i = 0; i < points.Length; i++) {
                Context.Vertices.Add(new(points[i] + LowLevel.CurrentCoordinateOffset, color));
                Context.Indices.Add((ushort)(Context.Vertices.Count - 1));
            }

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = (uint)points.Length,
                IndexLocation = (ushort)icount,

                ScissorsRect = LowLevel.CurrentScissorRect,
                Topology = D3D11_PRIMITIVE_TOPOLOGY.LineStrip,
            });
        }
        public static void DrawFrame(Rect rect, Color32 color) {
            if (ImGui.CurrentWindow == null || color.A == 0 || rect.HasInvalidSize) return;

            Vector2 min = rect.Position + LowLevel.CurrentCoordinateOffset + new Vector2(0.5f), max = rect.Max + LowLevel.CurrentCoordinateOffset - new Vector2(0.49f);

            var vcount = Context.Vertices.Count;
            var icount = Context.Indices.Count;

            Context.Vertices.Add(new Vertex(min, color));
            Context.Vertices.Add(new Vertex(new Vector2(max.X, min.Y), color));
            Context.Vertices.Add(new Vertex(max, color));
            Context.Vertices.Add(new Vertex(new Vector2(min.X, max.Y), color));

            Context.Indices.Add((ushort)vcount);
            Context.Indices.Add((ushort)(vcount + 1));
            Context.Indices.Add((ushort)(vcount + 2));
            Context.Indices.Add((ushort)(vcount + 3));
            Context.Indices.Add((ushort)vcount);

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = 5,
                IndexLocation = (ushort)icount,

                ScissorsRect = LowLevel.CurrentScissorRect,
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

            var vcount = Context.Vertices.Count;
            var icount = Context.Indices.Count;

            // Outer vertices
            Vector2 min = rect.Position + LowLevel.CurrentCoordinateOffset + new Vector2(0.5f), max = rect.Max + LowLevel.CurrentCoordinateOffset - new Vector2(0.49f);
            Context.Vertices.Add(new Vertex(min, color));
            Context.Vertices.Add(new Vertex(new Vector2(max.X, min.Y), color));
            Context.Vertices.Add(new Vertex(max, color));
            Context.Vertices.Add(new Vertex(new Vector2(min.X, max.Y), color));
            // Inner vertices
            rect.Extrude(-thickness);
            min = rect.Position + LowLevel.CurrentCoordinateOffset + new Vector2(0.5f);
            max = rect.Max + LowLevel.CurrentCoordinateOffset - new Vector2(0.49f);

            Context.Vertices.Add(new Vertex(min, color));
            Context.Vertices.Add(new Vertex(new Vector2(max.X, min.Y), color));
            Context.Vertices.Add(new Vertex(max, color));
            Context.Vertices.Add(new Vertex(new Vector2(min.X, max.Y), color));

            Context.Indices.Add((ushort)vcount);
            Context.Indices.Add((ushort)(vcount + 4));
            Context.Indices.Add((ushort)(vcount + 1));
            Context.Indices.Add((ushort)(vcount + 5));
            Context.Indices.Add((ushort)(vcount + 2));
            Context.Indices.Add((ushort)(vcount + 6));
            Context.Indices.Add((ushort)(vcount + 3));
            Context.Indices.Add((ushort)(vcount + 7));
            Context.Indices.Add((ushort)vcount);
            Context.Indices.Add((ushort)(vcount + 4));

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = 10,
                IndexLocation = (ushort)icount,

                ScissorsRect = LowLevel.CurrentScissorRect,
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

            var vcount = Context.Vertices.Count;
            var icount = Context.Indices.Count;

            circle.Center += LowLevel.CurrentCoordinateOffset;

            float angleStep = MathF.PI * 2 / numSegments;

            for (int i = 0; i < numSegments; i++) {
                (float sin, float cos) = MathF.SinCos(angleStep * i);

                Context.Vertices.Add(new(new Vector2(cos, -sin) * circle.Radius + circle.Center, color));
                Context.Vertices.Add(new(new Vector2(cos, -sin) * (circle.Radius - thickness) + circle.Center, color));
            }

            Context.Indices.Add((ushort)vcount);
            Context.Indices.Add((ushort)(vcount + 1));
            Context.Indices.Add((ushort)(vcount + 2));

            for (int i = 3; i < numSegments * 2; i++) {
                Context.Indices.Add((ushort)(vcount + i));
            }

            Context.Indices.Add((ushort)vcount);
            Context.Indices.Add((ushort)(vcount + 1));

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = (ushort)(Context.Indices.Count - icount),
                IndexLocation = (ushort)icount,

                ScissorsRect = LowLevel.CurrentScissorRect,
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

            var vcount = Context.Vertices.Count;
            var icount = Context.Indices.Count;

            circle.Center += LowLevel.CurrentCoordinateOffset;

            float angleStep = MathF.PI * 2 * normalize / numSegments;
            numSegments += 1;

            for (int i = 0; i < numSegments; i++) {
                (float sin, float cos) = MathF.SinCos(start + angleStep * i);

                Context.Vertices.Add(new(new Vector2(cos, -sin) * circle.Radius + circle.Center, color));
                Context.Vertices.Add(new(new Vector2(cos, -sin) * (circle.Radius - thickness) + circle.Center, color));
            }

            Context.Indices.Add((ushort)vcount);
            Context.Indices.Add((ushort)(vcount + 1));
            Context.Indices.Add((ushort)(vcount + 2));

            for (int i = 3; i < numSegments * 2; i++) {
                Context.Indices.Add((ushort)(vcount + i));
            }

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = (ushort)(Context.Indices.Count - icount),
                IndexLocation = (ushort)icount,

                ScissorsRect = LowLevel.CurrentScissorRect,
                Topology = D3D11_PRIMITIVE_TOPOLOGY.TriangleStrip,
            });
        }
        public static void DrawTexturedRect(Vertex topLeft, Vertex topRight, Vertex bottomRight, Vertex bottomLeft, DDTexture2D texture) {
            if (!texture.IsRenderable) {
                Logger.Warn("Texture used for rendering isn't flagged as Renderable");
                return;
            }

            LowLevel.DrawTexturedRect(topLeft, topRight, bottomRight, bottomLeft, texture.DXSRV, texture.DXSampler);
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
                    var icount = Context.Indices.Count;

                    var rectMax = rect.Max;

                    if (keys[0].Position != 0) {
                        switch (keys[0].Mode) {
                            default:
                                segmentWidth = rect.Width * keys[0].NormalizedPosition;

                                vcount = Context.Vertices.Count;

                                Context.Vertices.Add(new(rect.Position + LowLevel.CurrentCoordinateOffset, keys[0].Color));
                                Context.Vertices.Add(new(new Vector2(rect.X + segmentWidth, rect.Y) + LowLevel.CurrentCoordinateOffset, keys[0].Color));
                                Context.Vertices.Add(new(new Vector2(rect.X + segmentWidth, rectMax.Y) + LowLevel.CurrentCoordinateOffset, keys[0].Color));
                                Context.Vertices.Add(new(new Vector2(rect.X, rectMax.Y) + LowLevel.CurrentCoordinateOffset, keys[0].Color));

                                Context.Indices.Add((ushort)(vcount + 3));
                                Context.Indices.Add((ushort)vcount);
                                Context.Indices.Add((ushort)(vcount + 2));
                                Context.Indices.Add((ushort)(vcount + 1));
                                break;

                            case GradientColorMode.Fixed: {
                                segmentWidth = rect.Width * keys[1].NormalizedPosition;

                                vcount = Context.Vertices.Count;

                                Context.Vertices.Add(new(rect.Position + LowLevel.CurrentCoordinateOffset, keys[0].Color));
                                Context.Vertices.Add(new(new Vector2(rect.X + segmentWidth, rect.Y) + LowLevel.CurrentCoordinateOffset, keys[0].Color));
                                Context.Vertices.Add(new(new Vector2(rect.X + segmentWidth, rectMax.Y) + LowLevel.CurrentCoordinateOffset, keys[0].Color));
                                Context.Vertices.Add(new(new Vector2(rect.X, rectMax.Y) + LowLevel.CurrentCoordinateOffset, keys[0].Color));

                                Context.Indices.Add((ushort)(vcount + 3));
                                Context.Indices.Add((ushort)vcount);
                                Context.Indices.Add((ushort)(vcount + 2));
                                Context.Indices.Add((ushort)(vcount + 1));

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

                        vcount = Context.Vertices.Count;

                        Context.Vertices.Add(new Vertex(new Vector2(xPos, rect.Y) + LowLevel.CurrentCoordinateOffset, keys[i].Color));
                        Context.Vertices.Add(new Vertex(new Vector2(xPos, rectMax.Y) + LowLevel.CurrentCoordinateOffset, keys[i].Color));

                        Context.Indices.Add((ushort)(vcount + 1));
                        Context.Indices.Add((ushort)vcount);

                        xPos += segmentSize;

                        if (keys[i].Mode == GradientColorMode.Fixed) {
                            Context.Vertices.Add(new Vertex(new Vector2(xPos, rect.Y) + LowLevel.CurrentCoordinateOffset, keys[i].Color));
                            Context.Vertices.Add(new Vertex(new Vector2(xPos, rectMax.Y) + LowLevel.CurrentCoordinateOffset, keys[i].Color));

                            Context.Indices.Add((ushort)(vcount + 3));
                            Context.Indices.Add((ushort)(vcount + 2));
                        }
                    }

                    vcount = Context.Vertices.Count;

                    Context.Vertices.Add(new Vertex(new Vector2(xPos, rect.Y) + LowLevel.CurrentCoordinateOffset, keys[^1].Color));
                    Context.Vertices.Add(new Vertex(new Vector2(xPos, rectMax.Y) + LowLevel.CurrentCoordinateOffset, keys[^1].Color));

                    Context.Vertices.Add(new Vertex(new Vector2(rect.Width, rect.Y) + LowLevel.CurrentCoordinateOffset, keys[^1].Color));
                    Context.Vertices.Add(new Vertex(new Vector2(rect.Width, rectMax.Y) + LowLevel.CurrentCoordinateOffset, keys[^1].Color));

                    Context.Indices.Add((ushort)(vcount + 1));
                    Context.Indices.Add((ushort)vcount);
                    Context.Indices.Add((ushort)(vcount + 3));
                    Context.Indices.Add((ushort)(vcount + 2));

                    ImGui.CurrentWindow.DrawCalls.Add(new DrawCall() {
                        IndexCount = (uint)(Context.Indices.Count - icount),
                        IndexLocation = (uint)icount,

                        ScissorsRect = LowLevel.CurrentScissorRect,
                        Topology = D3D11_PRIMITIVE_TOPOLOGY.TriangleStrip,
                    });

                    break;
                }
            }
        }

        public static void AddCompositeRect(Rect rect, Color32 color) {
            if (ImGui.CurrentWindow == null || !ImGui.CurrentWindow.IsDrawingComposite || color.A == 0 || rect.HasInvalidSize) return;

            var vcount = Context.Vertices.Count;

            var min = rect.Position + LowLevel.CurrentCoordinateOffset;
            var max = rect.Max + LowLevel.CurrentCoordinateOffset;

            LowLevel.AddVertices(new Vertex(min, color));
            LowLevel.AddVertices(new Vertex(new Vector2(max.X, min.Y), color));
            LowLevel.AddVertices(new Vertex(max, color));
            LowLevel.AddVertices(new Vertex(new Vector2(min.X, max.Y), color));

            LowLevel.AddIndices(vcount);
            LowLevel.AddIndices(vcount + 1);
            LowLevel.AddIndices(vcount + 2);
            LowLevel.AddIndices(vcount + 2);
            LowLevel.AddIndices(vcount + 3);
            LowLevel.AddIndices(vcount);
        }

        public static void AddCompositeCircle(Circle circle, Color32 color, int numSegments) {
            if (ImGui.CurrentWindow == null || circle.Radius < 1 || circle.Radius < 1) return;

            numSegments = Math.Clamp(numSegments, 3, MaximumCircleSegment);

            var vcount = Context.Vertices.Count;

            circle.Center += LowLevel.CurrentCoordinateOffset;

            float angleStep = MathF.PI * 2 / numSegments;
            for (int i = 0; i < numSegments; i++) {
                (float sin, float cos) = MathF.SinCos(angleStep * i);
                Context.Vertices.Add(new(new Vector2(cos, -sin) * circle.Radius + circle.Center, color));
            }

            for (int i = 0; i < numSegments - 2; i++) {
                Context.Indices.Add((ushort)vcount);
                Context.Indices.Add((ushort)(vcount + i + 1));
                Context.Indices.Add((ushort)(vcount + i + 2));
            }
        }

        public static void AddCompositeOutlinedCircle(Circle circle, float thickness, Color32 color, int numSegments) {
            if (ImGui.CurrentWindow == null || circle.Radius < 1 || color.A == 0 || thickness <= 0.5f) return;

            if (thickness >= circle.Radius) {
                AddCompositeCircle(circle, color, numSegments);
                return;
            }

            numSegments = Math.Clamp(numSegments, 3, MaximumCircleSegment);

            var vcount = Context.Vertices.Count;

            circle.Center += LowLevel.CurrentCoordinateOffset;

            float angleStep = MathF.PI * 2 / numSegments;

            for (int i = 0; i < numSegments; i++) {
                (float sin, float cos) = MathF.SinCos(angleStep * i);

                Context.Vertices.Add(new(new Vector2(cos, -sin) * circle.Radius + circle.Center, color));
                Context.Vertices.Add(new(new Vector2(cos, -sin) * (circle.Radius - thickness) + circle.Center, color));
            }

            for (int i = 0; i < numSegments - 1; i++) {
                int outsideIndex = vcount + i * 2;

                Context.Indices.Add((ushort)outsideIndex);
                Context.Indices.Add((ushort)(outsideIndex + 1));
                Context.Indices.Add((ushort)(outsideIndex + 2));
                Context.Indices.Add((ushort)(outsideIndex + 2));
                Context.Indices.Add((ushort)(outsideIndex + 3));
                Context.Indices.Add((ushort)(outsideIndex + 1));
            }

            int last = vcount + (numSegments - 1) * 2;
            Context.Indices.Add((ushort)last);
            Context.Indices.Add((ushort)(last + 1));
            Context.Indices.Add((ushort)vcount);
            Context.Indices.Add((ushort)vcount);
            Context.Indices.Add((ushort)(vcount + 1));
            Context.Indices.Add((ushort)(last + 1));
        }

        /// <summary>
        /// Draw a character at given position. Bottom left anchor
        /// </summary>
        public static void AddCharacterQuad_Unchecked(float xPosition, float yPosition, char character, Color32 color) {
            if (FontStack.Current.TryGetPackedChar(character, out var output)) {
                AddCharacterQuad_Raw(xPosition, yPosition, output, FontStack.Current.BitmapUVStep, color);
            }
        }

        /// <summary>
        /// Draw a character at given position. Bottom left anchor
        /// </summary>
        public static void AddCharacterQuad_Raw(float xPosition, float yPosition, in stbtt_packedchar pCharacterQuad, Vector2 uvStep, Color32 color) {
            float minX = MathF.Floor(xPosition + pCharacterQuad.xoff + 0.5f);
            float minY = MathF.Floor(yPosition + pCharacterQuad.yoff + 0.5f);

            float maxX = (int)(minX + pCharacterQuad.xoff2 - pCharacterQuad.xoff);
            float maxY = (int)(minY + pCharacterQuad.yoff2 - pCharacterQuad.yoff);

            Vector2 minUV = new Vector2(pCharacterQuad.x0, pCharacterQuad.y0) * uvStep;
            Vector2 maxUV = new Vector2(pCharacterQuad.x1, pCharacterQuad.y1) * uvStep;

            var vcount = Context.Vertices.Count;

            LowLevel.AddVertices(new Vertex(new(minX, minY), color, minUV));
            LowLevel.AddVertices(new Vertex(new(maxX, minY), color, new Vector2(maxUV.X, minUV.Y)));
            LowLevel.AddVertices(new Vertex(new(maxX, maxY), color, maxUV));
            LowLevel.AddVertices(new Vertex(new(minX, maxY), color, new Vector2(minUV.X, maxUV.Y)));

            LowLevel.AddIndex(vcount);
            LowLevel.AddIndex(vcount + 1);
            LowLevel.AddIndex(vcount + 2);
            LowLevel.AddIndex(vcount + 2);
            LowLevel.AddIndex(vcount + 3);
            LowLevel.AddIndex(vcount);
        }
    }
}
