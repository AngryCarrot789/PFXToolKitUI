// 
// Copyright (c) 2023-2025 REghZy
// 
// This file is part of PFXToolKitUI.
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3 of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with PFXToolKitUI. If not, see <https://www.gnu.org/licenses/>.
// 

using System.Xml;
using PFXToolKitUI.Persistence.Serialisation;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Persistence;

/// <summary>
/// The base class for a persistent property registration. These properties are used to define a serialisable property
/// </summary>
public abstract class PersistentProperty {
    private static readonly ReaderWriterLockSlim RegistryLock = new ReaderWriterLockSlim();
    private static readonly Dictionary<string, PersistentProperty> RegistryMap = new Dictionary<string, PersistentProperty>();
    private static readonly Dictionary<Type, List<PersistentProperty>> TypeToParametersMap = new Dictionary<Type, List<PersistentProperty>>();
    private static int NextGlobalIndex = 1;

    private int runtimeId;
    private Type ownerType;
    private string name;
    private string globalKey;
    internal IList<string>? myDescription;

    /// <summary>
    /// Gets a unique name for this property, relative to the owner configuration type
    /// </summary>
    public string Name => this.name;

    /// <summary>
    /// Gets the type that this property is defined in
    /// </summary>
    public Type OwnerType => this.ownerType;

    /// <summary>
    /// Gets the globally registered index of this property. This is the only property used for equality
    /// comparison between parameters for speed purposes. There is a chance this value will not remain constant
    /// as the application is developed, due to the order in which properties are registered
    /// </summary>
    public int RuntimeId => this.runtimeId;

    /// <summary>
    /// Returns a string that is a concatenation of our owner type's simple name and our key, joined by '::'.
    /// This is a globally unique value, and no two parameters can be registered with the same global keys
    /// </summary>
    public string GlobalKey => this.globalKey;

    /// <summary>
    /// A list of lines added before the property definition in the configuration. Care must be taken not
    /// to use an invalid XML characters or text sequences (e.g. '-->' at the end of a line)
    /// </summary>
    public IList<string> DescriptionLines {
        get {
            if (this.myDescription == null) {
                ObservableList<string> list = new ObservableList<string>();
                ObservableItemProcessor.MakeSimple(list, s => {
                    if (s == null!)
                        throw new InvalidOperationException("Cannot add null string to list");
                }, null);

                this.myDescription = list;
            }

            return this.myDescription;
        }
    }

    /// <summary>
    /// Registers a property for a type that is parsable from a string (e.g. int, uint, byte, decimal and so on)
    /// </summary>
    /// <param name="name">The name of the property</param>
    /// <param name="defaultValue">The property's default value</param>
    /// <param name="getValue">The getter</param>
    /// <param name="setValue">The setter</param>
    /// <param name="canSaveDefault">When true, a configuration's value of this property can be serialised even if the current value is the default value</param>
    /// <typeparam name="TValue">The value type</typeparam>
    /// <typeparam name="TOwner">The owner of the property</typeparam>
    /// <returns>The property, which you can store as a public static readonly field</returns>
    public static PersistentProperty<TValue> RegisterParsable<TValue, TOwner>(string name, TValue defaultValue, Func<TOwner, TValue> getValue, Action<TOwner, TValue> setValue, bool canSaveDefault) where TValue : IParsable<TValue> where TOwner : PersistentConfiguration {
        PersistentPropertyStringParsable<TValue> property = new PersistentPropertyStringParsable<TValue>(defaultValue, (x) => getValue((TOwner) x), (x, y) => setValue((TOwner) x, y), (x) => TValue.Parse(x, null), null, canSaveDefault);
        RegisterCore(property, name, typeof(TOwner));
        return property;
    }

    /// <summary>
    /// Registers a string property that represents a primitive numeric value (e.g. int, uint, byte, decimal and so on)
    /// </summary>
    /// <param name="name">The name of the property</param>
    /// <param name="defaultValue">The property's default value</param>
    /// <param name="getValue">The getter</param>
    /// <param name="setValue">The setter</param>
    /// <param name="canSaveDefault">When true, a configuration's value of this property can be serialised even if the current value is the default value</param>
    /// <typeparam name="TOwner">The owner of the property</typeparam>
    /// <returns>The property, which you can store as a public static readonly field</returns>
    public static PersistentProperty<string> RegisterString<TOwner>(string name, string defaultValue, Func<TOwner, string> getValue, Action<TOwner, string> setValue, bool canSaveDefault) where TOwner : PersistentConfiguration {
        PersistentPropertyStringParsable<string> property = new PersistentPropertyStringParsable<string>(defaultValue, (x) => getValue((TOwner) x), (x, y) => setValue((TOwner) x, y), (x) => x, null, canSaveDefault);
        RegisterCore(property, name, typeof(TOwner));
        return property;
    }

    /// <summary>
    /// Registers an enum property
    /// </summary>
    /// <param name="name">The name of the property</param>
    /// <param name="defaultValue">The property's default value</param>
    /// <param name="getValue">The getter</param>
    /// <param name="setValue">The setter</param>
    /// <param name="canSaveDefault">When true, a configuration's value of this property can be serialised even if the current value is the default value</param>
    /// <typeparam name="TEnum">The enum type</typeparam>
    /// <typeparam name="TOwner">The owner of the property</typeparam>
    /// <returns>The property, which you can store as a public static readonly field</returns>
    public static PersistentProperty<TEnum> RegisterEnum<TEnum, TOwner>(string name, TEnum defaultValue, Func<TOwner, TEnum> getValue, Action<TOwner, TEnum> setValue, bool useNumericValue, bool canSaveDefault) where TEnum : unmanaged, Enum where TOwner : PersistentConfiguration {
        PersistentPropertyEnum<TEnum> property = new PersistentPropertyEnum<TEnum>(defaultValue, (x) => getValue((TOwner) x), (x, y) => setValue((TOwner) x, y), canSaveDefault, useNumericValue);
        RegisterCore(property, name, typeof(TOwner));
        return property;
    }

    /// <summary>
    /// Registers a boolean property
    /// </summary>
    /// <param name="name">The name of the property</param>
    /// <param name="defaultValue">The property's default value</param>
    /// <param name="getValue">The getter</param>
    /// <param name="setValue">The setter</param>
    /// <param name="canSaveDefault">When true, a configuration's value of this property can be serialised even if the current value is the default value</param>
    /// <typeparam name="TOwner">The owner of the property</typeparam>
    /// <returns>The property, which you can store as a public static readonly field</returns>
    public static PersistentProperty<bool> RegisterBool<TOwner>(string name, bool defaultValue, Func<TOwner, bool> getValue, Action<TOwner, bool> setValue, bool canSaveDefault) where TOwner : PersistentConfiguration {
        return RegisterParsable(name, defaultValue, getValue, setValue, canSaveDefault);
    }

    public static PersistentProperty<string[]> RegisterStringArray<TOwner>(string name, string[]? defaultValue, Func<TOwner, string[]> getValue, Action<TOwner, string[]> setValue, bool canSaveDefault) where TOwner : PersistentConfiguration {
        PersistentPropertyStringArray property = new PersistentPropertyStringArray(defaultValue ?? Array.Empty<string>(), (x) => getValue((TOwner) x), (x, y) => setValue((TOwner) x, y), canSaveDefault);
        RegisterCore(property, name, typeof(TOwner));
        return property;
    }

    /// <summary>
    /// Registers a completely custom persistent property using the given XML serializer
    /// </summary>
    /// <param name="name">The name of the property</param>
    /// <param name="defaultValue">The property's default value</param>
    /// <param name="getValue">The getter</param>
    /// <param name="setValue">The setter</param>
    /// <param name="serializer"></param>
    /// <typeparam name="TValue">The value type</typeparam>
    /// <typeparam name="TOwner">The owner of the property</typeparam>
    /// <returns>The property, which you can store as a public static readonly field</returns>
    public static PersistentProperty<TValue> RegisterCustom<TValue, TOwner>(string name, TValue defaultValue, Func<TOwner, TValue> getValue, Action<TOwner, TValue> setValue, IValueSerializer<TValue> serializer) where TOwner : PersistentConfiguration {
        PersistentPropertyCustom<TValue> property = new PersistentPropertyCustom<TValue>(defaultValue, (x) => getValue((TOwner) x), (x, y) => setValue((TOwner) x, y), serializer);
        RegisterCore(property, name, typeof(TOwner));
        return property;
    }

    private static void RegisterCore(PersistentProperty property, string name, Type ownerType) {
        if (property.runtimeId != 0) {
            throw new InvalidOperationException($"Property '{property.globalKey}' was already registered with a global index of " + property.runtimeId);
        }

        RegistryLock.EnterWriteLock();

        try {
            string path = ownerType.Name + "::" + name;
            if (RegistryMap.TryGetValue(path, out PersistentProperty? existingProperty)) {
                throw new Exception($"Key already exists with the ID '{path}': {existingProperty}");
            }

            RegistryMap[path] = property;
            if (!TypeToParametersMap.TryGetValue(ownerType, out List<PersistentProperty>? list))
                TypeToParametersMap[ownerType] = list = new List<PersistentProperty>();

            list.Add(property);
            property.runtimeId = NextGlobalIndex++;
            property.name = name;
            property.ownerType = ownerType;
            property.globalKey = path;
        }
        finally {
            RegistryLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Serialise the current value into the given parent element.
    /// Returning false results in the parent element not being appended to the final document
    /// </summary>
    public abstract bool Serialize(PersistentConfiguration config, XmlDocument document, XmlElement propertyElement);

    /// <summary>
    /// Deserialise the value from the element into the config instance
    /// </summary>
    public abstract void Deserialize(PersistentConfiguration config, XmlElement propertyElement);

    private class PersistentPropertyCustom<T> : PersistentProperty<T> {
        private readonly IValueSerializer<T> serializer;

        public PersistentPropertyCustom(T defaultValue, Func<PersistentConfiguration, T> getValue, Action<PersistentConfiguration, T> setValue, IValueSerializer<T> serializer) : base(defaultValue, getValue, setValue) {
            this.serializer = serializer;
        }

        public override bool Serialize(PersistentConfiguration config, XmlDocument document, XmlElement propertyElement) {
            return this.serializer.Serialize(this.GetValue(config), document, propertyElement);
        }

        public override void Deserialize(PersistentConfiguration config, XmlElement propertyElement) {
            this.SetValue(config, this.serializer.Deserialize(propertyElement));
        }
    }

    private class PersistentPropertyStringParsable<T> : PersistentProperty<T> where T : IParsable<T> {
        private readonly Func<string, T> fromString;
        private readonly Func<T, string>? toString;
        private readonly string? defaultText;
        private readonly bool canSaveDefault;

        public PersistentPropertyStringParsable(T defaultValue, Func<PersistentConfiguration, T> getValue, Action<PersistentConfiguration, T> setValue, Func<string, T> fromString, Func<T, string>? toString = null, bool canSaveDefault = false) : base(defaultValue, getValue, setValue) {
            this.fromString = fromString;
            this.toString = toString;
            this.canSaveDefault = canSaveDefault;

            this.defaultText = this.toString != null ? this.toString(defaultValue) : defaultValue.ToString();
        }

        public override bool Serialize(PersistentConfiguration config, XmlDocument document, XmlElement propertyElement) {
            T value = this.GetValue(config);
            string? text = this.toString != null ? this.toString(value) : value.ToString();
            if (text == null || (!this.canSaveDefault && text == this.defaultText)) {
                return false;
            }

            propertyElement.SetAttribute("value", text);
            return true;
        }

        public override void Deserialize(PersistentConfiguration config, XmlElement propertyElement) {
            if (!(propertyElement.GetAttribute("value") is string text)) {
                throw new Exception("Missing 'value' attribute");
            }

            this.SetValue(config, this.fromString(text));
        }
    }

    private class PersistentPropertyStringArray : PersistentProperty<string[]> {
        private static readonly string[] empty = Array.Empty<string>();

        private readonly string[] defaultValue;
        private readonly bool canSaveDefault;

        public PersistentPropertyStringArray(string[] defaultValue, Func<PersistentConfiguration, string[]> getValue, Action<PersistentConfiguration, string[]> setValue, bool canSaveDefault = false) : base(defaultValue, getValue, setValue) {
            this.canSaveDefault = canSaveDefault;
            this.defaultValue = defaultValue;
        }

        public override bool Serialize(PersistentConfiguration config, XmlDocument document, XmlElement propertyElement) {
            string[] value = this.GetValue(config);
            if (value.Length < 1 || (!this.canSaveDefault && value.SequenceEqual(this.defaultValue))) {
                return false;
            }

            foreach (string element in value) {
                XmlElement elem = document.CreateElement("str");
                elem.InnerText = element;
                propertyElement.AppendChild(elem);
            }

            return true;
        }

        public override void Deserialize(PersistentConfiguration config, XmlElement propertyElement) {
            List<string> values = new List<string>();
            foreach (XmlElement elem in propertyElement.GetElementsByTagName("str").OfType<XmlElement>()) {
                values.Add(elem.InnerText);
            }

            this.SetValue(config, values.ToArray());
        }
    }

    private class PersistentPropertyEnum<T> : PersistentProperty<T> where T : unmanaged, Enum {
        private readonly bool canSaveDefault;
        private readonly bool useNumericValue;

        public PersistentPropertyEnum(T defaultValue, Func<PersistentConfiguration, T> getValue, Action<PersistentConfiguration, T> setValue, bool canSaveDefault = false, bool useNumericValue = false) : base(defaultValue, getValue, setValue) {
            this.canSaveDefault = canSaveDefault;
            this.useNumericValue = useNumericValue;
        }

        public override bool Serialize(PersistentConfiguration config, XmlDocument document, XmlElement propertyElement) {
            T value = this.GetValue(config);
            if (!this.canSaveDefault && EqualityComparer<T>.Default.Equals(this.DefaultValue, value)) {
                return false;
            }

            if (this.useNumericValue) {
                string text = EnumInfo<T>.IsUnsigned
                    ? EnumInfo<T>.GetUnsignedValue(value).ToString()
                    : EnumInfo<T>.GetSignedValue(value).ToString();
                propertyElement.SetAttribute("enum_number", text);
            }
            else {
                if (!(Enum.GetName(value) is string text)) {
                    throw new Exception("Current enum value is invalid");
                }

                propertyElement.SetAttribute("enum_name", text);
            }

            return true;
        }

        public override void Deserialize(PersistentConfiguration config, XmlElement propertyElement) {
            T value;
            if (this.useNumericValue) {
                if (!(propertyElement.GetAttribute("enum_number") is string text)) {
                    throw new Exception("Missing 'enum_number' attribute");
                }

                if (EnumInfo<T>.IsUnsigned) {
                    if (!ulong.TryParse(text, out ulong value_ul))
                        throw new Exception("Missing unsigned 'enum_number' value: " + text);
                    value = EnumInfo<T>.FromUnsignedValue(value_ul);
                }
                else {
                    if (!long.TryParse(text, out long value_sl))
                        throw new Exception("Missing signed 'enum_number' value: " + text);
                    value = EnumInfo<T>.FromSignedValue(value_sl);
                }

                if (!EnumInfo<T>.IsValid(value))
                    throw new Exception("Invalid enum value: " + text);
            }
            else {
                if (!(propertyElement.GetAttribute("enum_name") is string text)) {
                    throw new Exception("Missing 'enum_name' attribute");
                }

                if (!Enum.TryParse(text, out value)) {
                    throw new Exception("Enum value does not exist: " + text);
                }
            }

            this.SetValue(config, value);
        }
    }

    public static List<PersistentProperty> GetProperties(Type type, bool baseTypes) {
        List<PersistentProperty> props = new List<PersistentProperty>();
        
        RegistryLock.EnterReadLock();
        
        try {
            for (Type? theType = type; theType != null && theType != typeof(PersistentConfiguration); theType = theType.BaseType) {
                if (TypeToParametersMap.TryGetValue(theType, out List<PersistentProperty>? properties)) {
                    props.AddRange(properties);
                }

                if (!baseTypes) {
                    break;
                }
            }
        }
        finally {
            RegistryLock.ExitReadLock();
        }

        return props;
    }

    internal abstract void InternalAssignDefaultValue(PersistentConfiguration config);

    public bool IsValidForConfiguration(PersistentConfiguration configuration) {
        return this.OwnerType.IsInstanceOfType(configuration);
    }

    public void ValidateIsValidForConfiguration(PersistentConfiguration configuration) {
        if (!this.IsValidForConfiguration(configuration))
            throw new InvalidOperationException($"Configuration ({configuration.GetType().Name}) does not own this property '{this.GlobalKey}'");
    }

    public void CheckIsValid() {
        if (this.runtimeId == 0 || string.IsNullOrWhiteSpace(this.name) || this.ownerType == null)
            throw new InvalidOperationException("This property has been unsafely created and is invalid");
    }
}

public delegate void PersistentPropertyInstanceValueChangeEventHandler<T>(PersistentConfiguration config, PersistentProperty<T> property, T oldValue, T newValue);

/// <summary>
/// The main implementation for a persistent property
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class PersistentProperty<T> : PersistentProperty {
    private readonly Func<PersistentConfiguration, T> getter;
    private readonly Action<PersistentConfiguration, T> setter;

    /// <summary>
    /// Gets the default value for this property
    /// </summary>
    public T DefaultValue { get; }

    protected PersistentProperty(T defaultValue, Func<PersistentConfiguration, T> getValue, Action<PersistentConfiguration, T> setValue) {
        this.DefaultValue = defaultValue;
        this.getter = getValue;
        this.setter = setValue;
    }

    public T GetValue(PersistentConfiguration config) => this.getter(config ?? throw new ArgumentNullException(nameof(config), "Config cannot be null"));

    public void SetValue(PersistentConfiguration config, T value) {
        if (config == null)
            throw new ArgumentNullException(nameof(config), "Config cannot be null");

        T oldValue = this.GetValue(config);
        if (!EqualityComparer<T>.Default.Equals(oldValue, value)) {
            config.internalIsModified = true;
            PersistentConfiguration.InternalRaiseValueChange(config, this, oldValue, value, true);
            this.setter(config, value);
            PersistentConfiguration.InternalRaiseValueChange(config, this, oldValue, value, false);
        }
    }

    internal override void InternalAssignDefaultValue(PersistentConfiguration config) {
        this.setter(config, this.DefaultValue);
    }

    /// <summary>
    /// Adds a value change handler for the given property for this configuration instance
    /// </summary>
    /// <param name="configuration">The configuration whose value must change to invoke the handler</param>
    /// <param name="handler">The handler to invoke when the property changes for this configuration instance</param>
    /// <param name="isBeforeChange">True to handle ValueChanging, False to handle ValueChanged.<para>Default value is false</para></param>
    public void AddValueChangeHandler(PersistentConfiguration configuration, PersistentPropertyInstanceValueChangeEventHandler<T>? handler, bool isBeforeChange = false) {
        configuration.AddValueChangeHandler(this, handler, isBeforeChange);
    }

    /// <summary>
    /// Removes the value change handler for the given property for this configuration instance
    /// </summary>
    /// <param name="configuration">The configuration whose value must change to invoke the handler</param>
    /// <param name="handler">The handler to invoke when the property changes for this configuration instance</param>
    /// <param name="isBeforeChange">True for the ValueChanging event, False for the ValueChanged event.<para>Default value is false</para></param>
    public void RemoveValueChangeHandler(PersistentConfiguration configuration, PersistentPropertyInstanceValueChangeEventHandler<T>? handler, bool isBeforeChange = false) {
        configuration.RemoveValueChangeHandler(this, handler, isBeforeChange);
    }
}