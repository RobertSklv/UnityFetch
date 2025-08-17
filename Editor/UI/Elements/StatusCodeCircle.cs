using UnityEngine.UIElements;

namespace UnityFetch.Editor.UI.Elements
{
    [UxmlElement]
    public partial class StatusCodeCircle : VisualElement
    {
        public const string Class = "status-code-circle";
        public const string InfoClass = "status-code-circle--info";
        public const string SuccessClass = "status-code-circle--success";
        public const string WarningClass = "status-code-circle--warning";
        public const string DangerClass = "status-code-circle--danger";

        [UxmlAttribute]
        public Type ColorType { get; set; }

        public StatusCodeCircle()
        {
            UnregisterCallback<AttachToPanelEvent>(AttachToPanel);

            RegisterCallback<AttachToPanelEvent>(AttachToPanel);
        }

        private void AttachToPanel(AttachToPanelEvent evt)
        {
            AddToClassList(Class);

            EnableInClassList(InfoClass, ColorType == Type.Info);
            EnableInClassList(SuccessClass, ColorType == Type.Success);
            EnableInClassList(WarningClass, ColorType == Type.Warning);
            EnableInClassList(DangerClass, ColorType == Type.Danger);
        }

        public enum Type
        {
            None,
            Info,
            Success,
            Warning,
            Danger
        }
    }
}