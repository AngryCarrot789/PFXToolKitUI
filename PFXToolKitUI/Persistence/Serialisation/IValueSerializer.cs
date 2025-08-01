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

namespace PFXToolKitUI.Persistence.Serialisation;

/// <summary>
/// An interface that implements serialisation and deserialization logic from the raw serialisation context
/// </summary>
/// <typeparam name="T">The serialisable value</typeparam>
public interface IValueSerializer<T> {
    /// <summary>
    /// Serialise the value as XML
    /// </summary>
    /// <param name="value">The value to serialise</param>
    /// <param name="document">The XML document involved</param>
    /// <param name="parent">The parent element. Add your child xml objects to this</param>
    /// <returns>False results in the parent element not being appended to the final document</returns>
    bool Serialize(T value, XmlDocument document, XmlElement parent);

    /// <summary>
    /// Deserialise this value from the element
    /// </summary>
    /// <param name="element">The xml element</param>
    /// <returns>The deserialised value</returns>
    T Deserialize(XmlElement element);
}