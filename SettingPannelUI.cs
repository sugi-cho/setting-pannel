using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class SettingPannelUI : MonoBehaviour
{
    [SerializeField] Test test;
    VisualElement settingPannel;
    private void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        settingPannel = new VisualElement();
        root.Add(settingPannel);
    }

    delegate VisualElement PropertyVisualElement(object data, PropertyInfo info);
    static Dictionary<System.Type, PropertyVisualElement> propertyVisualElementMap
        = new Dictionary<System.Type, PropertyVisualElement>
        {
            {
                typeof(bool), (data, info) =>
                {
                    var toggle = new Toggle(info.Name);
                    toggle.value = (bool)info.GetValue(data);
                    toggle.RegisterValueChangedCallback(evt => info.SetValue(data, evt.newValue));
                    return toggle;
                }
            },
            {
                typeof(string), (data, info) =>
                {
                    var field = new TextField(info.Name);
                    field.value = (string)info.GetValue(data);
                    field.isDelayed = true;
                    field.RegisterValueChangedCallback(evt => info.SetValue(data, evt.newValue));
                    return field;
                }
            },
            {
                typeof(int), (data, info) =>
                {
                    var field = new TextField(info.Name);
                    field.value = ((int)info.GetValue(data)).ToString();
                    field.isDelayed = true;
                    field.RegisterValueChangedCallback(evt =>
                    {
                        if(int.TryParse(evt.newValue, out int val))
                            info.SetValue(data, val);
                        else
                            field.value = evt.previousValue;
                    });
                    return field;
                }
            },
            {
                typeof(float), (data, info) =>
                {
                    var field = new TextField(info.Name);
                    field.value = ((float)info.GetValue(data)).ToString();
                    field.isDelayed = true;
                    field.RegisterValueChangedCallback(evt =>
                    {
                        if(float.TryParse(evt.newValue, out float val))
                            info.SetValue(data, val);
                        else
                            field.value = evt.previousValue;
                    });
                    return field;
                }
            },
            //typeof(Vector2), maybe later
            //typeof(Vector3),
            //typeof(Vector4),
            //typeof(Color),
        };

    public void SetupSettingUI<T>(T data) where T : class
    {
        var scroll = new ScrollView();
        var t = data.GetType();
        var typeInfo = t.GetTypeInfo();
        var fileName = $"{typeInfo}.json";

        if (!Directory.Exists(Application.streamingAssetsPath))
            Directory.CreateDirectory(Application.streamingAssetsPath);
        var filePath = Path.Combine(Application.streamingAssetsPath, fileName);
        if (File.Exists(filePath))
        {
            var json = File.ReadAllText(filePath);
            JsonUtility.FromJsonOverwrite(json, data);
        }
        else
            SaveData();

        var propertyInfos = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        System.Array.ForEach(propertyInfos, info =>
        {
            var type = info.PropertyType;
            if (propertyVisualElementMap.ContainsKey(type))
            {
                var ve = propertyVisualElementMap[type].Invoke(data, info);
                scroll.Add(ve);
            }
        });

        settingPannel.Add(scroll);

        Application.quitting += SaveData;
        void SaveData()
        {
            var json = JsonUtility.ToJson(data);
            File.WriteAllText(filePath, json);
        }
    }
    public void SetVisible(bool show)
    {
        if (show)
            settingPannel.style.display = DisplayStyle.Flex;
        else
            settingPannel.style.display = DisplayStyle.None;
    }

    [System.Serializable]
    public class Test
    {
        [SerializeField] string m_name;
        [SerializeField] int m_num;
        [SerializeField] float m_val;
        public string Name { get => m_name; set => m_name = value; }
        public int Num { get => m_num; set => m_num = value; }
        public float Val { get => m_val; set => m_val = value; }
    }
}
