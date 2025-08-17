using UnityEngine.UIElements;

namespace UnityFetch.Editor.UI.Elements
{
    [UxmlElement]
    public partial class KeyValueField : VisualElement
    {
        public const string Class = "key-value-field";
        public const string KeyClass = Class + "__key";
        public const string ValueClass = Class + "__value";

        private readonly string key;
        private readonly VisualElement decoration;
        private readonly string value;

        public KeyValueField()
        {
            Init();
        }

        public KeyValueField(string key, VisualElement decoration, string value)
        {
            this.key = key;
            this.decoration = decoration;
            this.value = value;

            Init();
        }

        public KeyValueField(string key, string value)
            : this(key, null, value)
        {
        }

        public KeyValueField(string key, object value)
            : this(key, value.ToString())
        {
        }

        private void Init()
        {
            AddToClassList(Class);

            Label keyLabel = new(key);
            keyLabel.AddToClassList(KeyClass);

            Label valueLabel = new(value);
            valueLabel.AddToClassList(ValueClass);

            Add(keyLabel);
            if (decoration != null) Add(decoration);
            Add(valueLabel);
        }
    }
}