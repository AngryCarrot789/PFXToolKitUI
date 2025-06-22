using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace PFXToolKitUI.Avalonia.Utils;

public class AutoTemplate {
    private static readonly Dictionary<Type, bool> CachedUseAutoTemplate = new Dictionary<Type, bool>();
    private static readonly Dictionary<Type, List<(string, FieldInfo)>> CachedTypeToFieldsToApply = new Dictionary<Type, List<(string, FieldInfo)>>();

    private static readonly AttachedProperty<bool> IsTemplateAppliedManuallyProperty = AvaloniaProperty.RegisterAttached<AutoTemplate, TemplatedControl, bool>("IsTemplateApplied");

    static AutoTemplate() {
        // TemplatedControl.TemplateAppliedEvent.AddClassHandler<TemplatedControl>(OnTemplateApplied);
    }

    private static void OnTemplateApplied(TemplatedControl control, TemplateAppliedEventArgs e) {
        if (control.GetValue(IsTemplateAppliedManuallyProperty)) {
            control.SetValue(IsTemplateAppliedManuallyProperty, false);
            return;
        }

        Type type = control.GetType();
        if (!CachedUseAutoTemplate.TryGetValue(type, out bool useAutoTemplate)) {
            CachedUseAutoTemplate[type] = useAutoTemplate = control.GetType().IsDefined(typeof(TemplatedControlOwnerAttribute), true);
        }

        if (useAutoTemplate)
            ApplyTemplateInternal(control, e.NameScope);
    }

    public static void ApplyTemplateInternal(TemplatedControl control, INameScope scope) {
        List<(string, FieldInfo)> list = GetCache(control);
        foreach ((string PartName, FieldInfo Field) x in list) {
            object? foundControl = scope.Find(x.PartName);
            if (foundControl == null)
                throw new Exception($"Missing templated part '{x.PartName}' for control type {x.Field.DeclaringType?.Name ?? "ERROR"}");
            if (!x.Field.FieldType.IsInstanceOfType(foundControl))
                throw new Exception($"Templated part '{x.PartName}' is incompatible for field type '{x.Field.FieldType}' declared in control type {x.Field.DeclaringType?.Name ?? "ERROR"}");

            IntPtr handle = x.Field.FieldHandle.Value;
            x.Field.SetValue(control, foundControl);
        }

        control.SetValue(IsTemplateAppliedManuallyProperty, true);
    }

    private static List<(string, FieldInfo)> GetCache(TemplatedControl control) {
        if (!CachedTypeToFieldsToApply.TryGetValue(control.GetType(), out List<(string, FieldInfo)>? list)) {
            CachedTypeToFieldsToApply[control.GetType()] = list = new List<(string, FieldInfo)>();
            foreach (FieldInfo field in control.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                TemplatedControlAttribute? attribute = field.GetCustomAttribute<TemplatedControlAttribute>();
                if (attribute == null)
                    continue;
                list.Add((attribute.PartName ?? field.Name, field));
            }
        }

        return list;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class TemplatedControlAttribute : Attribute {
    public string? PartName { get; }

    public TemplatedControlAttribute() {
    }

    public TemplatedControlAttribute(string partName) {
        this.PartName = partName;
    }

    static TemplatedControlAttribute() {
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class TemplatedControlOwnerAttribute : Attribute;