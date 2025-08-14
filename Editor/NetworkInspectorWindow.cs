using System;
using System.Collections.Generic;
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

        private VisualElement rootElement;

        private VisualElement overview;

        private MultiColumnListView multiColumnListView;

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

            overview = rootElement.Q("overview");

            VisualElement panelRoot = rootElement.Q("panel-root");

            multiColumnListView = new()
            {
                bindingPath = "requests",
                showBoundCollectionSize = false,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };
            multiColumnListView.style.flexBasis = new StyleLength(new Length(50, LengthUnit.Percent));

            multiColumnListView.columns.Add(CreateColumn("method", "Method", 7, 3, 10));
            multiColumnListView.columns.Add(CreateColumn("url", "Url", 65, 15, 90));
            multiColumnListView.columns.Add(CreateColumn("status", "Status", 5, 4, 8));
            multiColumnListView.columns.Add(CreateColumn("type", "Type", 15, 5, 15, rightAlignment: true));
            multiColumnListView.columns.Add(CreateColumn("size", "Size", 5, 5, 12, rightAlignment: true));
            multiColumnListView.columns.Add(CreateColumn("time", "Time", 10, 5, 15, rightAlignment: true));

            panelRoot.Add(multiColumnListView);

            var so = new SerializedObject(networkSO);
            multiColumnListView.Bind(so);
            multiColumnListView.bindingPath = "requests";

            multiColumnListView.selectionChanged += OnRequestSelectionChange;
        }

        void OnDisable()
        {
            if (multiColumnListView != null)
            {
                multiColumnListView.selectionChanged -= OnRequestSelectionChange;
            }
        }

        public Column CreateColumn(
            string name,
            string title,
            float width,
            float minWidth,
            float maxWidth,
            bool rightAlignment = false)
        {
            return new Column
            {
                name = name,
                bindingPath = name,
                title = title,
                stretchable = true,
                width = new Length(width, LengthUnit.Percent),
                minWidth = new Length(minWidth, LengthUnit.Percent),
                maxWidth = new Length(maxWidth, LengthUnit.Percent),
                makeCell = () =>
                {
                    Label label = new();

                    label.style.unityTextAlign = new StyleEnum<TextAnchor>(
                        rightAlignment
                        ? TextAnchor.MiddleRight
                        : TextAnchor.MiddleLeft);
                    label.style.textOverflow = new StyleEnum<TextOverflow>(TextOverflow.Ellipsis);
                    
                    return label;
                }
            };
        }

        public void OnRequestSelectionChange(IEnumerable<object> selectedItems)
        {
            SelectRequest(networkSO.requests[multiColumnListView.selectedIndex]);
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
            payloadField.value = "";
            responseField.value = "";

            if (request == null)
            {
                return;
            }

            KeyValueField url = new("Request URL", request.url);
            KeyValueField method = new("Request Method", request.method);
            KeyValueField statusCode = new("Status Code", request.status);
            KeyValueField remoteAddress = new("Remote Address", request.remoteAddress);
            KeyValueField referrerPolicy = new("Referrer Policy", request.referrerPolicy);

            general.Add(url);
            general.Add(method);
            general.Add(statusCode);
            general.Add(remoteAddress);
            general.Add(referrerPolicy);

            request.requestHeaders.ForEach(h => requestHeaders.Add(new KeyValueField(h.name, h.value)));
            request.responseHeaders.ForEach(h => responseHeaders.Add(new KeyValueField(h.name, h.value)));

            payloadField.value = request.requestBody?.ToString();
            responseField.value = request.responseBody?.ToString();
        }
    }
}