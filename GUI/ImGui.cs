using DirectDimensional.Core;
using DirectDimensional.Bindings.WinAPI;
using DirectDimensional.Core.Utilities;
using System.Numerics;
using StbTrueTypeSharp;

using static StbTrueTypeSharp.StbTrueType;

namespace DirectDimensional.Editor.GUI {
    public static unsafe class ImGui {
        public static void DrawRect(Rect rect, Color32 color) {
            var vcount = ImGuiContext.Vertices.Count;
            var icount = ImGuiContext.Indices.Count;

            var min = rect.Min;
            var max = rect.Max;

            ImGuiContext.Vertices.Add(new Vertex(min.V3(), color));
            ImGuiContext.Vertices.Add(new Vertex(new Vector3(max.X, min.Y, 0), color));
            ImGuiContext.Vertices.Add(new Vertex(max.V3(), color));
            ImGuiContext.Vertices.Add(new Vertex(new Vector3(min.X, max.Y, 0), color));

            ImGuiContext.Indices.Add((ushort)vcount);
            ImGuiContext.Indices.Add((ushort)(vcount + 1));
            ImGuiContext.Indices.Add((ushort)(vcount + 2));
            ImGuiContext.Indices.Add((ushort)(vcount + 2));
            ImGuiContext.Indices.Add((ushort)(vcount + 3));
            ImGuiContext.Indices.Add((ushort)vcount);

            ImGuiContext.DrawCalls.Add(new DrawCall {
                IndexCount = 6,
                IndexLocation = (ushort)icount,

                ScissorsRect = ImGuiLowLevel.ScissorRect,
            });
        }

        public static void DrawTexturedRect(Vertex topLeft, Vertex topRight, Vertex bottomRight, Vertex bottomLeft, Texture2D texture) {
            if (!texture.IsRenderable) {
                Logger.Warn("Texture used for rendering isn't flagged as Renderable");
                return;
            }

            ImGuiLowLevel.DrawTexturedRect(topLeft, topRight, bottomRight, bottomLeft, texture.DXSRV, texture.DXSampler);
        }

        public static void AddCompositeRect(Rect rect, Color32 color) {
            var vcount = ImGuiContext.Vertices.Count;

            var min = rect.Min;
            var max = rect.Max;

            ImGuiLowLevel.AddVertices(new Vertex(min.V3(), color));
            ImGuiLowLevel.AddVertices(new Vertex(new Vector3(max.X, min.Y, 0), color));
            ImGuiLowLevel.AddVertices(new Vertex(new Vector3(max, 0), color));
            ImGuiLowLevel.AddVertices(new Vertex(new Vector3(min.X, max.Y, 0), color));

            ImGuiLowLevel.AddIndices(vcount, vcount + 1, vcount + 2, vcount + 2, vcount + 3, vcount);
        }

        public static bool ResponsiveHoveringArea(string id, Rect rect, StandardCursorID cursor) {
            bool hover = false;
            Identifier.PushIdentifier(id);

            if (rect.Contains(Mouse.MousePosition)) {
                Identifier.SetHoveringID();
                EditorCursor.Cursor = cursor;

                hover = true;
            } else {
                if (Identifier.ClearCurrentHoveringID()) {
                    EditorCursor.Cursor = StandardCursorID.IDC_ARROW;
                }
            }

            Identifier.PopIdentifier();
            return hover;
        }
        public static bool DraggingArea(string id, Rect rect, out Vector2 drag) {
            bool isDragging = false;
            drag = default;

            Identifier.PushIdentifier(id);
            {
                if (Identifier.IsIDActived()) {
                    if (Mouse.LeftReleased) {
                        Identifier.ClearActiveID();
                    } else {
                        drag = Mouse.MouseMoveDelta;
                    }

                    isDragging = true;
                } else {
                    if (Mouse.LeftPressed) {
                        if (rect.Contains(Mouse.MousePosition)) {
                            Identifier.SetActiveID();
                        }
                    }
                }
            }
            Identifier.PopIdentifier();

            return isDragging;
        }
        public static bool ResponsiveDraggingArea(string id, Rect rect, StandardCursorID cursor, out Vector2 drag) {
            bool isDragging = false;
            drag = default;

            Identifier.PushIdentifier(id);

            if (Identifier.IsIDActived()) {
                EditorCursor.Cursor = cursor;

                drag = Mouse.MouseMoveDelta;
                isDragging = true;

                if (Mouse.LeftReleased) {
                    Identifier.ClearActiveID();
                    EditorCursor.Cursor = StandardCursorID.IDC_ARROW;
                }
            } else {
                if (rect.Contains(Mouse.MousePosition)) {
                    if (Mouse.LeftPressed) {
                        Identifier.SetActiveID();
                    }

                    Identifier.SetHoveringID();
                    EditorCursor.Cursor = cursor;
                } else {
                    if (Identifier.ClearCurrentHoveringID()) {
                        EditorCursor.Cursor = StandardCursorID.IDC_ARROW;
                    }
                }
            }

            Identifier.PopIdentifier();

            return isDragging;
        }
        public static bool DoResizingBorder(RectArea containerRect, float hresize, float vresize, ResizingBorderDirections direction, out Vector2 moveDelta, out ResizingBorderDirections dragDir) {
            moveDelta = default;
            dragDir = default;

            var cmin = containerRect.Min;
            var cmax = containerRect.Max;

            // Horizontals & Verticals
            if ((direction & ResizingBorderDirections.Left) == ResizingBorderDirections.Left) {
                if (ResponsiveDraggingArea("__RESIZING_BORDER_LEFT__", new RectArea(cmin - new Vector2(hresize, 0), new Vector2(cmin.X, cmax.Y)), StandardCursorID.IDC_SIZEWE, out var drag)) {
                    moveDelta = new Vector2(drag.X, 0);
                    dragDir = ResizingBorderDirections.Left;

                    Console.WriteLine("LEFT");
                    return true;
                }
            }

            if ((direction & ResizingBorderDirections.Right) == ResizingBorderDirections.Right) {
                if (ResponsiveDraggingArea("__RESIZING_BORDER_RIGHT__", new RectArea(new Vector2(cmax.X, cmin.Y), cmax + new Vector2(hresize, 0)), StandardCursorID.IDC_SIZEWE, out var drag)) {
                    moveDelta = new Vector2(drag.X, 0);
                    dragDir = ResizingBorderDirections.Right;
                    return true;
                }
            }

            if ((direction & ResizingBorderDirections.Bottom) == ResizingBorderDirections.Bottom) {
                if (ResponsiveDraggingArea("__RESIZING_BORDER_BOTTOM__", new RectArea(new Vector2(cmin.X, cmax.Y), new Vector2(cmax.X, cmax.Y + vresize)), StandardCursorID.IDC_SIZENS, out var drag)) {
                    moveDelta = new Vector2(0, drag.Y);
                    dragDir = ResizingBorderDirections.Bottom;
                    return true;
                }
            }

            if ((direction & ResizingBorderDirections.Top) == ResizingBorderDirections.Top) {
                if (ResponsiveDraggingArea("__RESIZING_BORDER_TOP__", new RectArea(new Vector2(cmin.X, cmin.Y - vresize), new Vector2(cmax.X, cmin.Y)), StandardCursorID.IDC_SIZENS, out var drag)) {
                    moveDelta = new Vector2(0, drag.Y);
                    dragDir = ResizingBorderDirections.Top;
                    return true;
                }
            }

            // Diagonals
            if ((direction & ResizingBorderDirections.TopLeft) == ResizingBorderDirections.TopLeft) {
                if (ResponsiveDraggingArea("__RESIZING_BORDER_TOPLEFT__", new RectArea(cmin - new Vector2(hresize, vresize), cmin), StandardCursorID.IDC_SIZENWSE, out var drag)) {
                    moveDelta = drag;
                    dragDir = ResizingBorderDirections.TopLeft;
                    return true;
                }
            }

            if ((direction & ResizingBorderDirections.BottomRight) == ResizingBorderDirections.BottomRight) {
                if (ResponsiveDraggingArea("__RESIZING_BORDER_BOTTOMRIGHT__", new RectArea(cmax, cmax + new Vector2(hresize, vresize)), StandardCursorID.IDC_SIZENWSE, out var drag)) {
                    moveDelta = drag;
                    dragDir = ResizingBorderDirections.BottomRight;
                    return true;
                }
            }

            return false;
        }

        public static void BeginWindow(string name, ref Vector2 windowPosition, ref Vector2 clientSize, float titlebarHeight) {
            Identifier.PushIdentifier(name);

            if (DraggingArea("__WND_TITLEBAR_DRAGMOVE__", new RectArea(windowPosition + new Vector2(2, 0), windowPosition + new Vector2(clientSize.X - 2, titlebarHeight)), out var tdelta)) {
                windowPosition += tdelta;
            }

            ImGuiLowLevel.BeginDrawComposite();

            var titlebarRect = new RectArea(windowPosition, windowPosition + new Vector2(clientSize.X, titlebarHeight));
            if (DoResizingBorder(new RectArea(windowPosition, windowPosition + clientSize + Vector2.UnitY * titlebarHeight), 5, 5, ResizingBorderDirections.All, out var delta, out var dir)) {
                switch (dir) {
                    case ResizingBorderDirections.Left:
                        windowPosition.X += delta.X;
                        clientSize.X -= delta.X;
                        break;

                    case ResizingBorderDirections.Right:
                        clientSize.X += delta.X;
                        break;

                    case ResizingBorderDirections.Top:
                        windowPosition.Y += delta.Y;
                        clientSize.Y -= delta.Y;
                        break;

                    case ResizingBorderDirections.Bottom:
                        clientSize.Y += delta.Y;
                        break;

                    case ResizingBorderDirections.TopLeft:
                        windowPosition += delta;
                        clientSize -= delta;
                        break;

                    case ResizingBorderDirections.BottomRight:
                        clientSize += delta;
                        break;
                }
            }

            AddCompositeRect(titlebarRect, ImGuiColoring.GetColor(ImGuiColoringID.WindowTitle));
            AddCompositeRect(new RectArea(new Vector2(windowPosition.X, windowPosition.Y + titlebarHeight), new Vector2(windowPosition.X, windowPosition.Y + titlebarHeight) + clientSize), ImGuiColoring.GetColor(ImGuiColoringID.WindowBackground));

            ImGuiLowLevel.BeginScissorRect(new Rect(windowPosition + new Vector2(clientSize.X / 2, titlebarHeight + clientSize.Y / 2), clientSize));

            ImGuiLowLevel.EndDrawComposite();
        }
        public static void EndWindow() {
            ImGuiLowLevel.EndScissorRect();

            Identifier.PopIdentifier();
        }

        public static void DrawText(ReadOnlySpan<char> str, Vector2 position) {
            var desc = EditorResources.FontBitmap.Description;

            ImGuiLowLevel.BeginDrawComposite();

            float stepX = 1f / desc.Width;
            float stepY = 1f / desc.Height;

            float scale = stbtt_ScaleForPixelHeight(EditorResources.FontInfo, EditorResources.FontPixelHeight);

            int ascent, descent, lineGap;
            stbtt_GetFontVMetrics(EditorResources.FontInfo, &ascent, &descent, &lineGap);

            float oldX = position.X;

            fixed (stbtt_packedchar* pPacked = &EditorResources.PackedChar[0]) {
                for (int i = 0; i < str.Length; i++) {
                    char c = str[i];

                    stbtt_packedchar* ptr = pPacked + c;

                    float charHeight = ptr->yoff2 - ptr->yoff;

                    float minX = MathF.Floor(position.X + ptr->xoff + 0.5f);
                    float minY = MathF.Floor(position.Y + ptr->yoff + 0.5f + (ascent - descent) * scale);

                    position.X += ptr->xadvance;

                    {
                        if (c == ' ' || c == '\t' || c == '\u3000' || c == '\r') continue;
                        if (c == '\n') {
                            position.X = oldX;
                            position.Y += (ascent - descent + lineGap) * scale;
                            continue;
                        }
                    }

                    float maxX = minX + ptr->xoff2 - ptr->xoff;
                    float maxY = minY + charHeight;

                    Vector2 minUV = new(ptr->x0 * stepX, ptr->y0 * stepY);
                    Vector2 maxUV = new(ptr->x1 * stepX, ptr->y1 * stepY);

                    var vcount = ImGuiLowLevel.VertexCount;
                    ImGuiLowLevel.AddVertex(new Vertex(new(minX, minY, 0), Color32.White, minUV));
                    ImGuiLowLevel.AddVertex(new Vertex(new(maxX, minY, 0), Color32.White, new(maxUV.X, minUV.Y)));
                    ImGuiLowLevel.AddVertex(new Vertex(new(maxX, maxY, 0), Color32.White, maxUV));
                    ImGuiLowLevel.AddVertex(new Vertex(new(minX, maxY, 0), Color32.White, new(minUV.X, maxUV.Y)));

                    ImGuiLowLevel.AddIndex(vcount);
                    ImGuiLowLevel.AddIndex(vcount + 1);
                    ImGuiLowLevel.AddIndex(vcount + 2);
                    ImGuiLowLevel.AddIndex(vcount + 2);
                    ImGuiLowLevel.AddIndex(vcount + 3);
                    ImGuiLowLevel.AddIndex(vcount);
                }
            }

            ImGuiLowLevel.EndDrawComposite(EditorResources.FontShaderResource, null);
        }
    }
}
