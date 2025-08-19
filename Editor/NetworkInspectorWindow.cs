using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityFetch.Debugging;
using UnityFetch.Editor.Scripts;
using UnityFetch.Editor.UI.Elements;

namespace UnityFetch.Editor
{
    public class NetworkInspectorWindow : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        [SerializeField]
        private NetworkSO networkSO;

        [SerializeField]
        private NetworkInspectorSettingsSO networkInspectorSettingsSO;

        private VisualElement rootElement;

        private TabView overview;

        private MultiColumnListView multiColumnListView;

        private Toolbar toolbar;

        private ToolbarButton clearRequestsButton;

        [MenuItem("Window/UnityFetch/Network Inspector")]
        public static void ShowWindow()
        {
            NetworkInspectorWindow wnd = GetWindow<NetworkInspectorWindow>();
            wnd.titleContent = new GUIContent("Network Inspector");
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            rootElement = m_VisualTreeAsset.Instantiate();

            root.Add(rootElement);

            overview = rootElement.Q<TabView>("overview");

            VisualElement panelRoot = rootElement.Q("panel-root");

            multiColumnListView = new()
            {
                name = "request-list",
                bindingPath = "requests",
                showBoundCollectionSize = false,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };
            multiColumnListView.style.flexBasis = new Length(100, LengthUnit.Percent);

            SerializedObject so = new(networkSO);
            multiColumnListView.columns.Add(CreateColumn(so, "method", "Method", 7, 5, 8));
            multiColumnListView.columns.Add(CreateColumn(so, "url", "Url", 56, 15, 90));
            multiColumnListView.columns.Add(CreateColumn(so, "statusLabel", "Status", 6, 4, 8));
            multiColumnListView.columns.Add(CreateColumn(so, "type", "Type", 7, 5, 9, rightAlignment: true));
            multiColumnListView.columns.Add(CreateColumn(so, "size", "Size", 8, 5, 11, rightAlignment: true));
            multiColumnListView.columns.Add(CreateColumn(so, "time", "Time", 8, 5, 11, rightAlignment: true));
            multiColumnListView.columns.Add(CreateColumn(so, "attempt", "Attempt", 8, 5, 11, rightAlignment: true));

            multiColumnListView.makeNoneElement = () => null;

            toolbar = new Toolbar();
            clearRequestsButton = new ToolbarButton();
            clearRequestsButton.text = "Clear";
            clearRequestsButton.clicked += ClearRequests;

            ToolbarMenu toolbarMenu = new();

            toolbarMenu.menu.AppendAction("Clear on Play", a =>
            {
                networkInspectorSettingsSO.clearOnPlay = !networkInspectorSettingsSO.clearOnPlay;
            },
            a => BooleanToDropdownMenuActionStatus(networkInspectorSettingsSO.clearOnPlay));
            toolbarMenu.menu.AppendAction("Clear on Build", a =>
            {
                networkInspectorSettingsSO.clearOnBuild = !networkInspectorSettingsSO.clearOnBuild;
            },
            a => BooleanToDropdownMenuActionStatus(networkInspectorSettingsSO.clearOnBuild));
            toolbarMenu.menu.AppendAction("Clear on Recompile", a =>
            {
                networkInspectorSettingsSO.clearOnRecompile = !networkInspectorSettingsSO.clearOnRecompile;
            },
            a => BooleanToDropdownMenuActionStatus(networkInspectorSettingsSO.clearOnRecompile));

            toolbar.Add(clearRequestsButton);
            toolbar.Add(toolbarMenu);
            panelRoot.Add(toolbar);
            panelRoot.Add(multiColumnListView);

            multiColumnListView.Bind(so);
            multiColumnListView.bindingPath = "requests";

            multiColumnListView.selectionChanged += OnRequestSelectionChange;

            UF.OnRequestFinish += RefreshRequestListItems;
        }

        void OnDisable()
        {
            if (multiColumnListView != null) multiColumnListView.selectionChanged -= OnRequestSelectionChange;
            if (clearRequestsButton != null) clearRequestsButton.clicked -= ClearRequests;

            UF.OnRequestFinish -= RefreshRequestListItems;
        }

        private DropdownMenuAction.Status BooleanToDropdownMenuActionStatus(bool val)
        {
            return val
                ? DropdownMenuAction.Status.Checked
                : DropdownMenuAction.Status.Normal;
        }

        private void ClearRequests()
        {
            networkSO.ClearRequests();
        }

        private void RefreshRequestListItems(UnityFetchRequestInfo requestInfo)
        {
            multiColumnListView.RefreshItems();
        }

        public string GetCellValue(int index, string name)
        {
            UnityFetchRequestInfo request = networkSO.requests[index];

            if (char.IsUpper(name[0]))
            {
                PropertyInfo prop = request.GetType().GetProperty(name);

                return prop.GetValue(request)?.ToString();
            }
            else
            {
                FieldInfo field = request.GetType().GetField(name);

                return field.GetValue(request)?.ToString();
            }
        }

        public Column CreateColumn(
            SerializedObject so,
            string name,
            string title,
            float width,
            float minWidth,
            float maxWidth,
            bool rightAlignment = false)
        {
            var col = new Column
            {
                name = name,
                title = title,
                stretchable = true,
                width = new Length(width, LengthUnit.Percent),
                minWidth = new Length(minWidth, LengthUnit.Percent),
                maxWidth = new Length(maxWidth, LengthUnit.Percent),
                bindCell = (VisualElement ve, int index) =>
                {
                    Label cell = ve as Label;

                    cell.BindProperty(
                        so.FindProperty("requests")
                            .GetArrayElementAtIndex(index)
                            .FindPropertyRelative(name)
                    );

                    if (networkSO.requests[index].IsFailed)
                    {
                        ve.parent.parent.AddToClassList("request-list-item--error");
                        ve.parent.parent.RemoveFromClassList("request-list-item--success");
                    }
                    else
                    {
                        ve.parent.parent.RemoveFromClassList("request-list-item--error");
                        ve.parent.parent.AddToClassList("request-list-item--success");
                    }
                },
                makeCell = () =>
                {
                    Label cell = new();
                    cell.AddToClassList("request-list-item__cell");

                    cell.AddToClassList(
                        rightAlignment
                        ? "request-list-item__cell--align-right"
                        : "request-list-item__cell--align-left");

                    return cell;
                }
            };

            return col;
        }

        public void OnRequestSelectionChange(IEnumerable<object> selectedItems)
        {
            if (multiColumnListView.selectedIndex == -1)
            {
                SelectRequest(null);
            }
            else
            {
                SelectRequest(networkSO.requests[multiColumnListView.selectedIndex]);
            }
        }

        public void ShowHideTab(Tab tab, bool visible)
        {
            StyleEnum<DisplayStyle> display = visible
                ? StyleKeyword.Null
                : DisplayStyle.None;

            if (!visible && overview.activeTab == tab)
            {
                foreach (var child in overview.Children())
                {
                    if (child is Tab t && t.tabHeader.resolvedStyle.display != DisplayStyle.None)
                    {
                        overview.activeTab = t;

                        break;
                    }
                }
            }

            tab.tabHeader.style.display = display;
        }

        public void SelectRequest(UnityFetchRequestInfo request)
        {
            Tab headers = overview.Q<Tab>("headers");
            Foldout general = headers.Query<Foldout>("general").First();
            Foldout requestHeaders = headers.Query<Foldout>("request-headers").First();
            Foldout responseHeaders = headers.Query<Foldout>("response-headers").First();
            Tab payload = overview.Q<Tab>("payload");
            TextField payloadField = payload.Query<TextField>("payload-field").First();
            Tab response = overview.Q<Tab>("response");
            TextField responseField = response.Query<TextField>("response-field").First();

            general.Clear();
            requestHeaders.Clear();
            responseHeaders.Clear();
            payloadField.value = string.Empty;
            responseField.value = string.Empty;

            if (request == null)
            {
                overview.style.visibility = Visibility.Hidden;
                ShowHideTab(headers, false);
                ShowHideTab(payload, false);
                ShowHideTab(response, false);

                return;
            }

            overview.style.visibility = Visibility.Visible;
            ShowHideTab(headers, true);

            if (!string.IsNullOrEmpty(request.requestBody))
            {
                ShowHideTab(payload, true);
            }
            else
            {
                ShowHideTab(payload, false);
            }

            if (!string.IsNullOrEmpty(request.responseBody))
            {
                ShowHideTab(response, true);
            }
            else
            {
                ShowHideTab(response, false);
            }

            KeyValueField url = new("Request URL", request.url);
            general.Add(url);
            KeyValueField method = new("Request Method", request.method);
            general.Add(method);

            if (request.status != 0)
            {
                string responseStatusLabel = Util.StatusCodeAsLabel(Util.StatusCodeToResponseStatus(request.status));

                StatusCodeCircle circle = new();

                if (request.status >= 400)
                {
                    circle.ColorType = StatusCodeCircle.Type.Danger;
                }
                else if (request.status >= 300)
                {
                    circle.ColorType = StatusCodeCircle.Type.Warning;
                }
                else if (request.status >= 200)
                {
                    circle.ColorType = StatusCodeCircle.Type.Success;
                }
                else if (request.status >= 100)
                {
                    circle.ColorType = StatusCodeCircle.Type.Info;
                }

                KeyValueField statusCode = new("Status Code", circle, request.status + " " + responseStatusLabel);
                general.Add(statusCode);
            }

            request.requestHeaders.ForEach(h => requestHeaders.Add(new KeyValueField(h.name, h.value)));
            request.responseHeaders.ForEach(h => responseHeaders.Add(new KeyValueField(h.name, h.value)));

            payloadField.value = request.requestBody?.ToString();
            responseField.value = request.responseBody?.ToString();
        }
    }
}