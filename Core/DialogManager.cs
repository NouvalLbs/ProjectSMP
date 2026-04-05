using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Display;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectSMP.Core
{
    public static class DialogManager
    {
        public static MessageDialogBuilder ShowMessage(this BasePlayer player, string title, string message)
            => new MessageDialogBuilder(player, title, message);

        public static InputDialogBuilder ShowInput(this BasePlayer player, string title, string message)
            => new InputDialogBuilder(player, title, message);

        public static ListDialogBuilder ShowList(this BasePlayer player, string title, params string[] items)
            => new ListDialogBuilder(player, title, items);

        public static TabListDialogBuilder ShowTabList(this BasePlayer player, string title, string[] headers)
            => new TabListDialogBuilder(player, title, headers);

        public static TabListNoHeaderDialogBuilder ShowTabListNoHeader(this BasePlayer player, string title, int columns = 1)
            => new TabListNoHeaderDialogBuilder(player, title, columns);

        public static PagedListDialogBuilder ShowPagedList(this BasePlayer player, string title, params string[] items)
            => new PagedListDialogBuilder(player, title, items);

        public static PagedTabListDialogBuilder ShowPagedTabList(this BasePlayer player, string title, string[] headers)
            => new PagedTabListDialogBuilder(player, title, headers);

        public class MessageDialogBuilder
        {
            private readonly BasePlayer _player;
            private readonly string _title;
            private readonly string _message;
            private string _btnLeft = "OK";
            private string _btnRight = "";

            internal MessageDialogBuilder(BasePlayer player, string title, string message)
            {
                _player = player;
                _title = title;
                _message = message;
            }

            public MessageDialogBuilder WithButtons(string left, string right = "")
            {
                _btnLeft = left;
                _btnRight = right;
                return this;
            }

            public void Show(Action<DialogResponseEventArgs> onResponse = null)
            {
                var dialog = new MessageDialog(_title, _message, _btnLeft, _btnRight);
                if (onResponse != null)
                    dialog.Response += (s, e) => onResponse(e);
                dialog.Show(_player);
            }

            public Task<DialogResponseEventArgs> ShowAsync()
            {
                var tcs = new TaskCompletionSource<DialogResponseEventArgs>();
                var dialog = new MessageDialog(_title, _message, _btnLeft, _btnRight);
                dialog.Response += (s, e) => tcs.TrySetResult(e);
                dialog.Show(_player);
                return tcs.Task;
            }
        }

        public class InputDialogBuilder
        {
            private readonly BasePlayer _player;
            private readonly string _title;
            private readonly string _message;
            private bool _isPassword;
            private string _btnLeft = "OK";
            private string _btnRight = "Batal";

            internal InputDialogBuilder(BasePlayer player, string title, string message)
            {
                _player = player;
                _title = title;
                _message = message;
            }

            public InputDialogBuilder AsPassword()
            {
                _isPassword = true;
                return this;
            }

            public InputDialogBuilder WithButtons(string left, string right = "Batal")
            {
                _btnLeft = left;
                _btnRight = right;
                return this;
            }

            public void Show(Action<DialogResponseEventArgs> onResponse = null)
            {
                var dialog = new InputDialog(_title, _message, _isPassword, _btnLeft, _btnRight);
                if (onResponse != null)
                    dialog.Response += (s, e) => onResponse(e);
                dialog.Show(_player);
            }

            public Task<DialogResponseEventArgs> ShowAsync()
            {
                var tcs = new TaskCompletionSource<DialogResponseEventArgs>();
                var dialog = new InputDialog(_title, _message, _isPassword, _btnLeft, _btnRight);
                dialog.Response += (s, e) => tcs.TrySetResult(e);
                dialog.Show(_player);
                return tcs.Task;
            }
        }

        public class ListDialogBuilder
        {
            private readonly BasePlayer _player;
            private readonly string _title;
            private readonly string[] _items;
            private string _btnLeft = "Pilih";
            private string _btnRight = "Batal";

            internal ListDialogBuilder(BasePlayer player, string title, string[] items)
            {
                _player = player;
                _title = title;
                _items = items;
            }

            public ListDialogBuilder WithButtons(string left, string right = "Batal")
            {
                _btnLeft = left;
                _btnRight = right;
                return this;
            }

            public void Show(Action<DialogResponseEventArgs> onResponse = null)
            {
                var dialog = new ListDialog(_title, _btnLeft, _btnRight);
                foreach (var item in _items)
                    dialog.AddItem(item);
                if (onResponse != null)
                    dialog.Response += (s, e) => onResponse(e);
                dialog.Show(_player);
            }

            public Task<DialogResponseEventArgs> ShowAsync()
            {
                var tcs = new TaskCompletionSource<DialogResponseEventArgs>();
                var dialog = new ListDialog(_title, _btnLeft, _btnRight);
                foreach (var item in _items)
                    dialog.AddItem(item);
                dialog.Response += (s, e) => tcs.TrySetResult(e);
                dialog.Show(_player);
                return tcs.Task;
            }
        }

        public class TabListDialogBuilder
        {
            private readonly BasePlayer _player;
            private readonly string _title;
            private readonly string[] _headers;
            private string[][] _rows = Array.Empty<string[]>();
            private string _btnLeft = "Pilih";
            private string _btnRight = "Batal";

            internal TabListDialogBuilder(BasePlayer player, string title, string[] headers)
            {
                _player = player;
                _title = title;
                _headers = headers;
            }

            public TabListDialogBuilder WithRows(params string[][] rows)
            {
                _rows = rows;
                return this;
            }

            public TabListDialogBuilder WithButtons(string left, string right = "Batal")
            {
                _btnLeft = left;
                _btnRight = right;
                return this;
            }

            public void Show(Action<DialogResponseEventArgs> onResponse = null)
            {
                var dialog = new TablistDialog(_title, _headers, _btnLeft, _btnRight);
                foreach (var row in _rows)
                    dialog.Add(row);
                if (onResponse != null)
                    dialog.Response += (s, e) => onResponse(e);
                dialog.Show(_player);
            }

            public Task<DialogResponseEventArgs> ShowAsync()
            {
                var tcs = new TaskCompletionSource<DialogResponseEventArgs>();
                var dialog = new TablistDialog(_title, _headers, _btnLeft, _btnRight);
                foreach (var row in _rows)
                    dialog.Add(row);
                dialog.Response += (s, e) => tcs.TrySetResult(e);
                dialog.Show(_player);
                return tcs.Task;
            }
        }

        public class TabListNoHeaderDialogBuilder
        {
            private readonly BasePlayer _player;
            private readonly string _title;
            private readonly int _columns;
            private string[][] _rows = Array.Empty<string[]>();
            private string _btnLeft = "Pilih";
            private string _btnRight = "Batal";

            internal TabListNoHeaderDialogBuilder(BasePlayer player, string title, int columns)
            {
                _player = player;
                _title = title;
                _columns = columns;
            }

            public TabListNoHeaderDialogBuilder WithItems(params string[][] rows)
            {
                _rows = rows;
                return this;
            }

            public TabListNoHeaderDialogBuilder WithButtons(string left, string right = "Batal")
            {
                _btnLeft = left;
                _btnRight = right;
                return this;
            }

            public void Show(Action<DialogResponseEventArgs> onResponse = null)
            {
                var dialog = new TablistDialog(_title, _columns, _btnLeft, _btnRight);
                foreach (var row in _rows)
                    dialog.Add(row);
                if (onResponse != null)
                    dialog.Response += (s, e) => onResponse(e);
                dialog.Show(_player);
            }

            public Task<DialogResponseEventArgs> ShowAsync()
            {
                var tcs = new TaskCompletionSource<DialogResponseEventArgs>();
                var dialog = new TablistDialog(_title, _columns, _btnLeft, _btnRight);
                foreach (var row in _rows)
                    dialog.Add(row);
                dialog.Response += (s, e) => tcs.TrySetResult(e);
                dialog.Show(_player);
                return tcs.Task;
            }
        }

        public class PagedListDialogBuilder
        {
            private const int ItemsPerPage = 10;
            private readonly BasePlayer _player;
            private readonly string _title;
            private readonly string[] _items;
            private string _btnLeft = "Select";
            private string _btnRight = "Close";
            private int _currentPage = 0;

            internal PagedListDialogBuilder(BasePlayer player, string title, string[] items)
            {
                _player = player;
                _title = title;
                _items = items;
            }

            public PagedListDialogBuilder WithButtons(string left, string right = "Close")
            {
                _btnLeft = left;
                _btnRight = right;
                return this;
            }

            public void Show(Action<DialogResponseEventArgs> onResponse = null)
            {
                ShowPage(_currentPage, onResponse);
            }

            private void ShowPage(int page, Action<DialogResponseEventArgs> onResponse)
            {
                _currentPage = page;
                var totalPages = (_items.Length + ItemsPerPage - 1) / ItemsPerPage;
                var startIdx = page * ItemsPerPage;
                var endIdx = Math.Min(startIdx + ItemsPerPage, _items.Length);

                var pageItems = new List<string>();

                for (var i = startIdx; i < endIdx; i++)
                    pageItems.Add(_items[i]);

                if (page > 0)
                    pageItems.Add("{FF6347}<< Previous");

                if (page < totalPages - 1)
                    pageItems.Add("{91ff00}>> Next");

                var dialog = new ListDialog($"{_title} (Page {page + 1}/{totalPages})", _btnLeft, _btnRight);
                foreach (var item in pageItems)
                    dialog.AddItem(item);

                dialog.Response += (s, e) =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        onResponse?.Invoke(e);
                        return;
                    }

                    var selectedText = pageItems[e.ListItem];

                    if (selectedText.Contains("<< Previous"))
                    {
                        ShowPage(page - 1, onResponse);
                        return;
                    }

                    if (selectedText.Contains(">> Next"))
                    {
                        ShowPage(page + 1, onResponse);
                        return;
                    }

                    onResponse?.Invoke(e);
                };

                dialog.Show(_player);
            }
        }

        public class PagedTabListDialogBuilder
        {
            private const int ItemsPerPage = 10;
            private readonly BasePlayer _player;
            private readonly string _title;
            private readonly string[] _headers;
            private string[][] _rows = Array.Empty<string[]>();
            private string _btnLeft = "Select";
            private string _btnRight = "Close";
            private int _currentPage = 0;

            internal PagedTabListDialogBuilder(BasePlayer player, string title, string[] headers)
            {
                _player = player;
                _title = title;
                _headers = headers;
            }

            public PagedTabListDialogBuilder WithRows(params string[][] rows)
            {
                _rows = rows;
                return this;
            }

            public PagedTabListDialogBuilder WithButtons(string left, string right = "Close")
            {
                _btnLeft = left;
                _btnRight = right;
                return this;
            }

            public void Show(Action<DialogResponseEventArgs> onResponse = null)
            {
                ShowPage(_currentPage, onResponse);
            }

            private void ShowPage(int page, Action<DialogResponseEventArgs> onResponse)
            {
                _currentPage = page;
                var totalPages = (_rows.Length + ItemsPerPage - 1) / ItemsPerPage;
                var startIdx = page * ItemsPerPage;
                var endIdx = Math.Min(startIdx + ItemsPerPage, _rows.Length);

                var pageRows = new List<string[]>();

                for (var i = startIdx; i < endIdx; i++)
                    pageRows.Add(_rows[i]);

                if (page > 0)
                {
                    var prevRow = new string[_headers.Length];
                    prevRow[0] = "{FF6347}<< Previous";
                    for (var i = 1; i < _headers.Length; i++)
                        prevRow[i] = "";
                    pageRows.Add(prevRow);
                }

                if (page < totalPages - 1)
                {
                    var nextRow = new string[_headers.Length];
                    nextRow[0] = "{91ff00}>> Next";
                    for (var i = 1; i < _headers.Length; i++)
                        nextRow[i] = "";
                    pageRows.Add(nextRow);
                }

                var dialog = new TablistDialog($"{_title} (Page {page + 1}/{totalPages})", _headers, _btnLeft, _btnRight);
                foreach (var row in pageRows)
                    dialog.Add(row);

                dialog.Response += (s, e) =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        onResponse?.Invoke(e);
                        return;
                    }

                    var selectedRow = pageRows[e.ListItem];

                    if (selectedRow[0].Contains("<< Previous"))
                    {
                        ShowPage(page - 1, onResponse);
                        return;
                    }

                    if (selectedRow[0].Contains(">> Next"))
                    {
                        ShowPage(page + 1, onResponse);
                        return;
                    }

                    onResponse?.Invoke(e);
                };

                dialog.Show(_player);
            }
        }
    }
}