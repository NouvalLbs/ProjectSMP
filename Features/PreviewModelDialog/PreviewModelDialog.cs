#nullable enable

using System;
using System.Collections.Generic;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Display;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;

namespace ProjectSMP.Features.PreviewModelDialog
{
    public static class PreviewModelDialog
    {
        public const int Rows = 2;
        public const int Cols = 6;
        public const int PageSize = Rows * Cols;

        private const float HGap = 56.5f;
        private const float VGap = 63.5f;
        private const float ScrollMaxH = 108f;
        private const int DoubleClickMs = 200;

        private static readonly Color SelectColor = new(0xFF, 0x00, 0x00, 0xFF);
        private static readonly Color UnselectColor = new(0x46, 0x46, 0x46, 0xFF);
        private static readonly Color White = new(0xFF, 0xFF, 0xFF, 0xFF);
        private static readonly Color Black = new(0x00, 0x00, 0x00, 0xFF);
        private static readonly Color SemiBlk150 = new(0x00, 0x00, 0x00, 150);
        private static readonly Color SemiBlk175 = new(0x00, 0x00, 0x00, 175);
        private static readonly Color PreviewBg = new(0x32, 0x32, 0x32, 0xFF);
        private static readonly Color WhiteA100 = new(0xFF, 0xFF, 0xFF, 100);
        private static readonly Color Transparent = new(0x00, 0x00, 0x00, 0x00);

        // Global TDs: frame + button backgrounds only (no per-slot overlapping elements)
        private static readonly TextDraw[] _frame = new TextDraw[8];
        private static readonly TextDraw[] _leftBtn = new TextDraw[2];
        private static readonly TextDraw[] _rightBtn = new TextDraw[2];
        private static readonly TextDraw[] _centerBtn = new TextDraw[2];
        // Global slot border (selectable hitbox), rendered BEFORE PlayerTextDraws
        private static readonly TextDraw[] _slotBorder = new TextDraw[PageSize];

        private static readonly Dictionary<int, Session> _sessions = new();

        public static void Init()
        {
            BuildFrame();
            BuildButtons();
            BuildSlotBorders();
        }

        public static void Dispose()
        {
            foreach (var td in _frame) td?.Dispose();
            foreach (var td in _leftBtn) td?.Dispose();
            foreach (var td in _rightBtn) td?.Dispose();
            foreach (var td in _centerBtn) td?.Dispose();
            foreach (var td in _slotBorder) td?.Dispose();
        }

        public static void Show(BasePlayer player, int dialogId, string caption,
            IList<PreviewModelItem> items, string button1, string button2 = "",
            Action<PreviewModelResponseArgs>? onResponse = null)
        {
            if (_sessions.ContainsKey(player.Id))
                HideInternal(player);

            var s = new Session
            {
                DialogId = dialogId,
                Items = new List<PreviewModelItem>(items),
                OnResponse = onResponse
            };
            _sessions[player.Id] = s;

            // Show global frame
            for (var i = 0; i < 8; i++) _frame[i].Show(player);

            // Show buttons
            if (string.IsNullOrEmpty(button2))
            {
                _leftBtn[0].Hide(player); _leftBtn[1].Hide(player);
                _rightBtn[0].Hide(player); _rightBtn[1].Hide(player);
                _centerBtn[0].Show(player); _centerBtn[1].Show(player);
            }
            else
            {
                _leftBtn[0].Show(player); _leftBtn[1].Show(player);
                _rightBtn[0].Show(player); _rightBtn[1].Show(player);
                _centerBtn[0].Hide(player); _centerBtn[1].Hide(player);
            }

            // Build all PlayerTextDraws (header, buttons text, slot PTDs)
            BuildPlayerTDs(player, s, caption, button1, button2);

            s.Header?.Show();
            s.PageNumber?.Show();
            if (string.IsNullOrEmpty(button2))
                s.CenterButton?.Show();
            else
            {
                s.LeftButton?.Show();
                s.RightButton?.Show();
            }

            if (s.NumPages > 1) UpdateScrollBar(player, s);

            UpdateListItems(player, s);

            player.SelectTextDraw(SelectColor);
        }

        public static void HandleClick(BasePlayer player, TextDraw? clicked)
        {
            if (!_sessions.TryGetValue(player.Id, out var s)) return;

            if (clicked == null || clicked == _rightBtn[0])
            {
                Respond(player, s, false);
                return;
            }

            if (clicked == _frame[4])
            {
                if (s.Page <= 0) return;
                s.Page--;
                s.SelectedSlot = 0;
                UpdateListItems(player, s);
                UpdateScrollBar(player, s);
                return;
            }

            if (clicked == _frame[6])
            {
                if (s.Page + 1 >= s.NumPages) return;
                s.Page++;
                s.SelectedSlot = 0;
                UpdateListItems(player, s);
                UpdateScrollBar(player, s);
                return;
            }

            if (clicked == _leftBtn[0] || clicked == _centerBtn[0])
            {
                Respond(player, s, true);
                return;
            }

            // Check slot border clicks
            for (var i = 0; i < PageSize; i++)
            {
                if (clicked != _slotBorder[i]) continue;

                var gIndex = (s.Page * PageSize) + i;
                if (gIndex >= s.TotalItems) break;

                if (s.SelectedSlot == i && Environment.TickCount - s.TickCount <= DoubleClickMs)
                {
                    Respond(player, s, true);
                }
                else
                {
                    DeselectSlot(player, s);
                    s.SelectedSlot = i;
                    s.TickCount = Environment.TickCount;
                    SelectSlot(player, s);
                }
                break;
            }
        }

        public static void HandlePlayerTextDrawClick(BasePlayer player, PlayerTextDraw? clicked)
        {
            if (!_sessions.TryGetValue(player.Id, out var s)) return;
            if (clicked == null) return;

            var slot = s.SelectedSlot;
            var gi = s.GlobalListItem;
            if (gi >= s.TotalItems) return;

            var item = s.Items[gi];

            if (clicked == s.SlotPTD[slot, 2]) { AdjustRotZ(player, s, item, -10f); return; }
            if (clicked == s.SlotPTD[slot, 3]) { AdjustRotZ(player, s, item, +10f); return; }
            if (clicked == s.SlotPTD[slot, 4]) { AdjustZoom(player, s, item, +0.1f); return; }
            if (clicked == s.SlotPTD[slot, 5]) { AdjustZoom(player, s, item, -0.1f); return; }
        }

        public static void HandleCancel(BasePlayer player)
        {
            if (!_sessions.TryGetValue(player.Id, out var s)) return;
            Respond(player, s, false);
        }

        // ── Session ───────────────────────────────────────────────────────────

        private sealed class Session
        {
            public int DialogId;
            public int SelectedSlot;
            public int Page;
            public int TickCount;
            public Action<PreviewModelResponseArgs>? OnResponse;
            public List<PreviewModelItem> Items = new();

            public PlayerTextDraw? Header;
            public PlayerTextDraw? LeftButton;
            public PlayerTextDraw? RightButton;
            public PlayerTextDraw? CenterButton;
            public PlayerTextDraw? PageNumber;
            public PlayerTextDraw? ScrollBar;

            // Per slot: [slot, 0]=model preview, [slot,1]=label text,
            //           [slot,2]=rotL btn, [slot,3]=rotR btn,
            //           [slot,4]=zoomIn btn, [slot,5]=zoomOut btn,
            //           [slot,6]=rotZ label, [slot,7]=zoom label
            public readonly PlayerTextDraw?[,] SlotPTD = new PlayerTextDraw?[PageSize, 8];

            public int TotalItems => Items.Count;
            public int NumPages => (TotalItems / PageSize) + (TotalItems % PageSize > 0 ? 1 : 0);
            public int GlobalListItem => (Page * PageSize) + SelectedSlot;
        }

        // ── Core Logic ────────────────────────────────────────────────────────

        private static void Respond(BasePlayer player, Session s, bool accepted)
        {
            var gi = s.GlobalListItem;
            var modelId = s.TotalItems > 0 ? s.Items[gi].ModelId : 0;
            var args = new PreviewModelResponseArgs(accepted, gi, modelId);
            var cb = s.OnResponse;
            HideInternal(player);
            cb?.Invoke(args);
        }

        private static void HideInternal(BasePlayer player)
        {
            if (!_sessions.TryGetValue(player.Id, out var s)) return;

            for (var i = 0; i < 8; i++) _frame[i].Hide(player);
            _leftBtn[0].Hide(player); _leftBtn[1].Hide(player);
            _rightBtn[0].Hide(player); _rightBtn[1].Hide(player);
            _centerBtn[0].Hide(player); _centerBtn[1].Hide(player);
            for (var i = 0; i < PageSize; i++) _slotBorder[i].Hide(player);

            DestroyPlayerTDs(s);
            _sessions.Remove(player.Id);
            player.CancelSelectTextDraw();
        }

        private static void AdjustRotZ(BasePlayer player, Session s, PreviewModelItem item, float delta)
        {
            item.RotZ += delta;
            if (item.RotZ <= -360f || item.RotZ >= 360f) item.RotZ = 0f;
            RecreateModelPreview(player, s, item);
            SetPTDText(s, s.SelectedSlot, 6, $"{item.RotZ:F1}");
            SetPTDText(s, s.SelectedSlot, 7, $"{item.Zoom:F1}");
        }

        private static void AdjustZoom(BasePlayer player, Session s, PreviewModelItem item, float delta)
        {
            var next = item.Zoom + delta;
            if (next < -5f || next > 5f) return;
            item.Zoom = next;
            RecreateModelPreview(player, s, item);
            SetPTDText(s, s.SelectedSlot, 6, $"{item.RotZ:F1}");
            SetPTDText(s, s.SelectedSlot, 7, $"{item.Zoom:F1}");
        }

        private static void RecreateModelPreview(BasePlayer player, Session s, PreviewModelItem item)
        {
            var slot = s.SelectedSlot;
            s.SlotPTD[slot, 0]?.Dispose();
            var b = slot % Cols;
            var a = slot / Cols;
            s.SlotPTD[slot, 0] = MakeModelPreviewPTD(player, b, a, item);
            s.SlotPTD[slot, 0]!.Show();
        }

        private static void DeselectSlot(BasePlayer player, Session s)
        {
            var slot = s.SelectedSlot;
            _slotBorder[slot].ForeColor = UnselectColor;
            _slotBorder[slot].Show(player);
            // Hide control buttons and labels
            s.SlotPTD[slot, 2]?.Hide();
            s.SlotPTD[slot, 3]?.Hide();
            s.SlotPTD[slot, 4]?.Hide();
            s.SlotPTD[slot, 5]?.Hide();
            s.SlotPTD[slot, 6]?.Hide();
            s.SlotPTD[slot, 7]?.Hide();
        }

        private static void SelectSlot(BasePlayer player, Session s)
        {
            var slot = s.SelectedSlot;
            var gi = (s.Page * PageSize) + slot;
            if (gi >= s.TotalItems) return;
            var item = s.Items[gi];

            _slotBorder[slot].ForeColor = SelectColor;
            _slotBorder[slot].Show(player);

            s.SlotPTD[slot, 2]?.Show();
            s.SlotPTD[slot, 3]?.Show();
            s.SlotPTD[slot, 4]?.Show();
            s.SlotPTD[slot, 5]?.Show();
            SetPTDText(s, slot, 6, $"{item.RotZ:F1}");
            SetPTDText(s, slot, 7, $"{item.Zoom:F1}");
        }

        private static void UpdateListItems(BasePlayer player, Session s)
        {
            for (var a = 0; a < Rows; a++)
            {
                for (var b = 0; b < Cols; b++)
                {
                    var slot = (a * Cols) + b;
                    var gi = (s.Page * PageSize) + slot;

                    if (gi >= s.TotalItems)
                    {
                        _slotBorder[slot].Hide(player);
                        for (var j = 0; j < 8; j++) s.SlotPTD[slot, j]?.Hide();
                        continue;
                    }

                    var item = s.Items[gi];
                    var selected = slot == s.SelectedSlot;

                    // Recreate model preview PTD fresh
                    s.SlotPTD[slot, 0]?.Dispose();
                    s.SlotPTD[slot, 0] = MakeModelPreviewPTD(player, b, a, item);
                    s.SlotPTD[slot, 0]!.Show();

                    _slotBorder[slot].ForeColor = selected ? SelectColor : UnselectColor;
                    _slotBorder[slot].Show(player);

                    if (selected)
                    {
                        s.SlotPTD[slot, 2]?.Show();
                        s.SlotPTD[slot, 3]?.Show();
                        s.SlotPTD[slot, 4]?.Show();
                        s.SlotPTD[slot, 5]?.Show();
                        SetPTDText(s, slot, 6, $"{item.RotZ:F1}");
                        SetPTDText(s, slot, 7, $"{item.Zoom:F1}");
                    }
                    else
                    {
                        s.SlotPTD[slot, 2]?.Hide();
                        s.SlotPTD[slot, 3]?.Hide();
                        s.SlotPTD[slot, 4]?.Hide();
                        s.SlotPTD[slot, 5]?.Hide();
                        s.SlotPTD[slot, 6]?.Hide();
                        s.SlotPTD[slot, 7]?.Hide();
                    }

                    if (!string.IsNullOrEmpty(item.Text))
                    {
                        if (s.SlotPTD[slot, 1] != null)
                        {
                            s.SlotPTD[slot, 1]!.Text = item.Text;
                            s.SlotPTD[slot, 1]!.Show();
                        }
                    }
                    else
                    {
                        s.SlotPTD[slot, 1]?.Hide();
                    }
                }
            }

            if (s.PageNumber != null)
                s.PageNumber.Text = $"PAGE: {s.Page + 1}/{s.NumPages}";
        }

        private static void UpdateScrollBar(BasePlayer player, Session s)
        {
            s.ScrollBar?.Dispose();
            var h = ScrollMaxH / s.NumPages;
            var y = 166.5f + h * s.Page;
            s.ScrollBar = new PlayerTextDraw(player, new Vector2(489f, y), "LD_SPAC:WHITE")
            {
                Font = TextDrawFont.DrawSprite,
                LetterSize = new Vector2(0f, 18.2f),
                ForeColor = SelectColor,
                Shadow = 0,
                Outline = 0,
                BackColor = Black,
                Proportional = true,
                UseBox = true,
                BoxColor = SemiBlk150,
                Width = 7.5f,
                Height = h
            };
            s.ScrollBar.Show();
        }

        private static void SetPTDText(Session s, int slot, int idx, string text)
        {
            var ptd = s.SlotPTD[slot, idx];
            if (ptd == null) return;
            ptd.Text = text;
            ptd.Show();
        }

        // ── PlayerTextDraw Builders ───────────────────────────────────────────

        private static PlayerTextDraw MakeModelPreviewPTD(BasePlayer player, int b, int a, PreviewModelItem item)
            => new(player, new Vector2(149f + b * HGap, 158f + a * VGap), "MODEL")
            {
                Font = TextDrawFont.PreviewModel,
                LetterSize = new Vector2(0.5f, 1f),
                ForeColor = White,
                Shadow = 0,
                Outline = 0,
                BackColor = PreviewBg,
                Proportional = true,
                Width = 54f,
                Height = 61f,
                PreviewModel = item.ModelId,
                PreviewRotation = new Vector3(item.RotX, item.RotY, item.RotZ),
                PreviewZoom = item.Zoom,
                PreviewPrimaryColor = item.Color1,
                PreviewSecondaryColor = item.Color2
            };

        private static void BuildPlayerTDs(BasePlayer player, Session s,
            string caption, string button1, string button2)
        {
            var hasDual = !string.IsNullOrEmpty(button2);

            s.Header = new PlayerTextDraw(player, new Vector2(144f, 143f), caption)
            {
                Font = TextDrawFont.Slim,
                LetterSize = new Vector2(0.1399f, 0.8999f),
                ForeColor = White,
                Shadow = 0,
                Outline = 0,
                BackColor = Black,
                Proportional = true,
                UseBox = true,
                BoxColor = Black,
                Width = 501f,
                Height = 0f
            };

            if (hasDual)
            {
                s.LeftButton = PTDCentered(player, new Vector2(287.5f, 297f), button1);
                s.RightButton = PTDCentered(player, new Vector2(352.5f, 297f), button2);
            }
            else
            {
                s.CenterButton = PTDCentered(player, new Vector2(318f, 297f), button1);
            }

            if (s.TotalItems > PageSize)
            {
                s.PageNumber = new PlayerTextDraw(player, new Vector2(501.5f, 143f), "Page: 1")
                {
                    Font = TextDrawFont.Slim,
                    LetterSize = new Vector2(0.1399f, 0.8999f),
                    Alignment = TextDrawAlignment.Right,
                    ForeColor = White,
                    Shadow = 0,
                    Outline = 0,
                    BackColor = Black,
                    Proportional = true,
                    Width = 501f,
                    Height = 0f
                };
            }

            for (var a = 0; a < Rows; a++)
            {
                for (var b = 0; b < Cols; b++)
                {
                    var slot = (a * Cols) + b;
                    if (slot >= s.TotalItems) continue;

                    // [1] = item label text
                    s.SlotPTD[slot, 1] = new PlayerTextDraw(player,
                        new Vector2(153.5f + b * HGap, 161.5f + a * VGap), " ")
                    {
                        Font = TextDrawFont.Slim,
                        LetterSize = new Vector2(0.1399f, 0.8999f),
                        ForeColor = White,
                        Shadow = 0,
                        Outline = 0,
                        BackColor = Black,
                        Proportional = true,
                        UseBox = true,
                        BoxColor = Transparent,
                        Width = 201.5f + b * HGap,
                        Height = 0f
                    };

                    // [2] rotate left, [3] rotate right, [4] zoom in, [5] zoom out
                    s.SlotPTD[slot, 2] = PTDBtn(player,
                        new Vector2(157.5f + b * HGap, 204f + a * VGap), "LD_BEAT:LEFT", sel: true);
                    s.SlotPTD[slot, 3] = PTDBtn(player,
                        new Vector2(185f + b * HGap, 204f + a * VGap), "LD_BEAT:RIGHT", sel: true);
                    s.SlotPTD[slot, 4] = PTDBtn(player,
                        new Vector2(191.5f + b * HGap, 171f + a * VGap), "LD_BEAT:UP", sel: true);
                    s.SlotPTD[slot, 5] = PTDBtn(player,
                        new Vector2(191.5f + b * HGap, 193f + a * VGap), "LD_BEAT:DOWN", sel: true);

                    // [6] rotZ label, [7] zoom label
                    s.SlotPTD[slot, 6] = new PlayerTextDraw(player,
                        new Vector2(176.5f + b * HGap, 204.5f + a * VGap), "0.0")
                    {
                        Font = TextDrawFont.Slim,
                        LetterSize = new Vector2(0.1399f, 0.8999f),
                        Alignment = TextDrawAlignment.Center,
                        ForeColor = WhiteA100,
                        Shadow = 0,
                        Outline = 0,
                        BackColor = Black,
                        Proportional = true,
                        Width = 640f,
                        Height = 480f
                    };

                    s.SlotPTD[slot, 7] = new PlayerTextDraw(player,
                        new Vector2(196.5f + b * HGap, 182f + a * VGap), "0.0")
                    {
                        Font = TextDrawFont.Slim,
                        LetterSize = new Vector2(0.1399f, 0.8999f),
                        Alignment = TextDrawAlignment.Center,
                        ForeColor = WhiteA100,
                        Shadow = 0,
                        Outline = 0,
                        BackColor = Black,
                        Proportional = true,
                        Width = 640f,
                        Height = 480f
                    };
                }
            }
        }

        private static PlayerTextDraw PTDCentered(BasePlayer owner, Vector2 pos, string text)
            => new(owner, pos, text)
            {
                Font = TextDrawFont.Slim,
                LetterSize = new Vector2(0.1399f, 0.8999f),
                Alignment = TextDrawAlignment.Center,
                ForeColor = White,
                Shadow = 0,
                Outline = 0,
                BackColor = Black,
                Proportional = true,
                Width = 0f,
                Height = 480f
            };

        private static PlayerTextDraw PTDBtn(BasePlayer owner, Vector2 pos, string sprite, bool sel = false)
            => new(owner, pos, sprite)
            {
                Font = TextDrawFont.DrawSprite,
                LetterSize = new Vector2(0.5f, 1f),
                ForeColor = WhiteA100,
                Shadow = 0,
                Outline = 0,
                BackColor = Black,
                Proportional = true,
                Width = 10f,
                Height = 10f,
                Selectable = sel
            };

        private static void DestroyPlayerTDs(Session s)
        {
            s.Header?.Dispose();
            s.LeftButton?.Dispose();
            s.RightButton?.Dispose();
            s.CenterButton?.Dispose();
            s.PageNumber?.Dispose();
            s.ScrollBar?.Dispose();
            for (var i = 0; i < PageSize; i++)
                for (var j = 0; j < 8; j++)
                    s.SlotPTD[i, j]?.Dispose();
        }

        // ── Global TextDraw Builders ──────────────────────────────────────────

        private static void BuildSlotBorders()
        {
            for (var a = 0; a < Rows; a++)
            {
                for (var b = 0; b < Cols; b++)
                {
                    var i = (a * Cols) + b;
                    _slotBorder[i] = new TextDraw(new Vector2(148.5f + b * HGap, 157.5f + a * VGap), "LD_SPAC:WHITE")
                    {
                        Font = TextDrawFont.DrawSprite,
                        LetterSize = new Vector2(0.5f, 1f),
                        ForeColor = UnselectColor,
                        Shadow = 0,
                        Outline = 0,
                        BackColor = Black,
                        Proportional = true,
                        Width = 55f,
                        Height = 62f,
                        Selectable = true
                    };
                }
            }
        }

        private static void BuildFrame()
        {
            _frame[0] = new TextDraw(new Vector2(144f, 143f), "CONTENT_BOX")
            {
                Font = TextDrawFont.Slim,
                LetterSize = new Vector2(0f, 19.5f),
                ForeColor = White,
                Shadow = 0,
                Outline = 0,
                BackColor = Black,
                Proportional = true,
                UseBox = true,
                BoxColor = SemiBlk175,
                Width = 501f,
                Height = 0f
            };
            _frame[1] = new TextDraw(new Vector2(146.5f, 155.5f), "LD_SPAC:WHITE")
            {
                Font = TextDrawFont.DrawSprite,
                LetterSize = new Vector2(0f, 18.2f),
                ForeColor = White,
                Shadow = 0,
                Outline = 0,
                BackColor = Black,
                Proportional = true,
                UseBox = true,
                BoxColor = SemiBlk150,
                Width = 351.5f,
                Height = 130f
            };
            _frame[2] = new TextDraw(new Vector2(147f, 156f), "LD_SPAC:BLACK")
            {
                Font = TextDrawFont.DrawSprite,
                LetterSize = new Vector2(0f, 18.2f),
                ForeColor = White,
                Shadow = 0,
                Outline = 0,
                BackColor = Black,
                Proportional = true,
                UseBox = true,
                BoxColor = SemiBlk150,
                Width = 340.5f,
                Height = 129f
            };
            _frame[3] = new TextDraw(new Vector2(488f, 156f), "LD_SPAC:BLACK")
            {
                Font = TextDrawFont.DrawSprite,
                LetterSize = new Vector2(0f, 18.2f),
                ForeColor = White,
                Shadow = 0,
                Outline = 0,
                BackColor = Black,
                Proportional = true,
                UseBox = true,
                BoxColor = SemiBlk150,
                Width = 9.5f,
                Height = 129f
            };
            _frame[4] = new TextDraw(new Vector2(488f, 156f), "LD_POOL:BALL")
            {
                Font = TextDrawFont.DrawSprite,
                LetterSize = new Vector2(0f, 18.2f),
                ForeColor = White,
                Shadow = 0,
                Outline = 0,
                BackColor = Black,
                Proportional = true,
                UseBox = true,
                BoxColor = SemiBlk150,
                Width = 9.5f,
                Height = 9.5f,
                Selectable = true
            };
            _frame[5] = new TextDraw(new Vector2(489f, 156.5f), "LD_BEAT:UP")
            {
                Font = TextDrawFont.DrawSprite,
                LetterSize = new Vector2(0f, 18.2f),
                ForeColor = Black,
                Shadow = 0,
                Outline = 0,
                BackColor = Black,
                Proportional = true,
                UseBox = true,
                BoxColor = SemiBlk150,
                Width = 7.5f,
                Height = 7.5f
            };
            _frame[6] = new TextDraw(new Vector2(488f, 275.5f), "LD_POOL:BALL")
            {
                Font = TextDrawFont.DrawSprite,
                LetterSize = new Vector2(0f, 18.2f),
                ForeColor = White,
                Shadow = 0,
                Outline = 0,
                BackColor = Black,
                Proportional = true,
                UseBox = true,
                BoxColor = SemiBlk150,
                Width = 9.5f,
                Height = 9.5f,
                Selectable = true
            };
            _frame[7] = new TextDraw(new Vector2(489f, 277f), "LD_BEAT:DOWN")
            {
                Font = TextDrawFont.DrawSprite,
                LetterSize = new Vector2(0f, 18.2f),
                ForeColor = Black,
                Shadow = 0,
                Outline = 0,
                BackColor = Black,
                Proportional = true,
                UseBox = true,
                BoxColor = SemiBlk150,
                Width = 7.5f,
                Height = 7.5f
            };
        }

        private static void BuildButtons()
        {
            _leftBtn[0] = TDBtn(new Vector2(260f, 293f), "LD_SPAC:WHITE", 55f, 18f, sel: true);
            _leftBtn[1] = TDBtn(new Vector2(260.5f, 293.5f), "LD_SPAC:BLACK", 54f, 17f);
            _rightBtn[0] = TDBtn(new Vector2(325f, 293f), "LD_SPAC:WHITE", 55f, 18f, sel: true);
            _rightBtn[1] = TDBtn(new Vector2(325.5f, 293.5f), "LD_SPAC:BLACK", 54f, 17f);
            _centerBtn[0] = TDBtn(new Vector2(290.5f, 293f), "LD_SPAC:WHITE", 55f, 18f, sel: true);
            _centerBtn[1] = TDBtn(new Vector2(291f, 293.5f), "LD_SPAC:BLACK", 54f, 17f);
        }

        private static TextDraw TDBtn(Vector2 pos, string text, float w, float h,
            Color color = default, bool sel = false)
            => new(pos, text)
            {
                Font = TextDrawFont.DrawSprite,
                LetterSize = new Vector2(0.5f, 1f),
                ForeColor = color.A == 0 ? White : color,
                Shadow = 0,
                Outline = 0,
                BackColor = Black,
                Proportional = true,
                Width = w,
                Height = h,
                Selectable = sel
            };
    }
}