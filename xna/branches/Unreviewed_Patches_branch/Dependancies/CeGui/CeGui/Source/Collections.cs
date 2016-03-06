#region LGPL License
/*************************************************************************
    Crazy Eddie's GUI System (http://crayzedsgui.sourceforge.net)
    Copyright (C)2004 Paul D Turner (crayzed@users.sourceforge.net)

    C# Port developed by Chris McGuirk (leedgitar@latenitegames.com)
    Compatible with the Axiom 3D Engine (http://axiomengine.sf.net)

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*************************************************************************/
#endregion LGPL License

using System;
using System.Collections;
using System.Text;

using CeGui.Widgets;

namespace CeGui {

#region WindowTable
public class WindowTable {
  SortedList list = new SortedList();

  public void Add(Window window) {
    if(window == null)
      throw new ArgumentException("Cannot add a null window.");
    list.Add(window.Name, window);
  }

  public void Clear() {
    list.Clear();
  }

  /// <summary>
  ///		Removes an item from the collection.
  /// </summary>
  /// <param name="name"></param>
  public void Remove(string name) {
    list.Remove(name);
  }


  /// <summary>
  ///		Removes an item from the collection.
  /// </summary>
  /// <param name="name"></param>
  public void Remove(Window window) {
    list.Remove(window.Name);
  }

  public bool Contains(string name) {
    return list.Contains(name);
  }

  public Window this[string name] {
    get {
      return (Window)list[name];
    }
  }

  public Window this[int index] {
    get {
      return (Window)list.GetByIndex(index);
    }
  }

  public int Count {
    get {
      return list.Count;
    }
  }
}
#endregion
#region WindowList Collection

public class WindowList : CollectionBase, ICollection {

  public void Add(Window window) {
    if(window == null)
      throw new ArgumentException("Cannot add a null window.");
    List.Add(window);
  }

  public void Insert(int position, Window window) {
    List.Insert(position, window);
  }

  /// <summary>
  ///		Removes an item from the collection.
  /// </summary>
  /// <param name="name"></param>
  public void Remove(string name) {
    Window window = null;

    for(int i = 0; i < List.Count; i++) {
      window = (Window)this.List[i];

      if(window.Name == name) {
        break;
      }
    }

    if(window != null) {
      List.Remove(window);
    }
  }
  public void Remove(Window window) {
    List.Remove(window);
  }

  public Window this[string name] {
    get {
      Window window;
      for(int i = 0; i < this.List.Count; i++) {
        window = (Window)this.List[i];
        if(window.Name == name)
          return window;
      }
      return null;
    }
    set {
      Window window;
      for(int i = 0; i < this.List.Count; i++) {
        window = (Window)this.List[i];
        if(window.Name == name) {
          window = value;
          break;
        }
      }
    }
  }
  public int IndexOf(Window window) {
    return (List.IndexOf(window));
  }

  public bool Contains(Window window) {
    return (List.Contains(window));
  }

  public Window this[int index] {
    get {
      return (Window)List[index];
    }
    set {
      List[index] = value;
    }
  }
/*
  public int Count {
    get { return List.Count; }
  }
*/
  void ICollection.CopyTo(Array array, int index) {
    this.List.CopyTo(array, index);
  }
  public void CopyTo(Window[] array, int index) {
    ((ICollection)this).CopyTo(array, index);
  }

  /// <summary>
  /// Copy array
  /// </summary>
  /// <param name="array">Array to copy collection to</param>
  /// <param name="index">Start index</param>
  public void CopyTo(Array array, int index) {
    this.List.CopyTo(array, index);
  }
}

#endregion WindowList Collection

#region ImagesetList Collection

public class ImagesetList {
  SortedList table = new SortedList();

  public void Add(string name, Imageset image) {
    table[name] = image;
  }

  public void Clear() {
    table.Clear();
  }

  /// <summary>
  ///		Removes an item from the collection.
  /// </summary>
  /// <param name="name"></param>
  public void Remove(string name) {
    if(table[name] != null) {
      table.Remove(name);
    }
  }

  public Imageset this[string name] {
    get {
      if(table[name] != null) {
        return (Imageset)table[name];
      }

      return null;
    }
    set {
      Add(name, value);
    }
  }

  public Imageset this[int index] {
    get {
      return (Imageset)table.GetByIndex(index);
    }
  }

  public int Count {
    get {
      return table.Count;
    }
  }
}

#endregion ImagesetList Collection

#region FontList Collection

public class FontList {
  ArrayList list = new ArrayList();

  public void Add(Font font) {
    list.Add(font);
  }

  public void Clear() {
    list.Clear();
  }

  public void Insert(int position, Window font) {
    list.Insert(position, font);
  }

  /// <summary>
  ///		Removes an item from the collection.
  /// </summary>
  /// <param name="name"></param>
  public void Remove(string name) {
    Font font = null;

    for(int i = 0; i < list.Count; i++) {
      font = (Font)list[i];

      if(font.Name == name) {
        break;
      }
    }

    if(font != null) {
      list.Remove(font);
    }
  }

  public Font this[string name] {
    get {
      Font font = null;

      for(int i = 0; i < list.Count; i++) {
        font = (Font)list[i];

        if(font.Name == name) {
          return font;
        }
      }

      return null;
    }
  }

  public Font this[int index] {
    get {
      return (Font)list[index];
    }
  }

  public int Count {
    get {
      return list.Count;
    }
  }
}

#endregion FontList Collection

#region ListboxItem Collection

public class ListboxItemList {
  ArrayList list = new ArrayList();

  public int Find(ListboxItem item) {
    return list.IndexOf(item);
  }

  public void Add(ListboxItem item) {
    list.Add(item);
  }

  public void Clear() {
    list.Clear();
  }

  public void Insert(int position, ListboxItem item) {
    list.Insert(position, item);
  }

  /// <summary>
  ///		Removes an item from the collection.
  /// </summary>
  /// <param name="name"></param>
  public void Remove(ListboxItem item) {
    list.Remove(item);
  }

  public void RemoveAt(int index) {
    list.RemoveAt(index);
  }

  public void Resize(int count) {
    // add some null entries...
    for(; list.Count < count; list.Add(null))
      ;
  }

  public void Sort() {
    list.Sort();
  }

  public int Count {
    get {
      return list.Count;
    }
  }

  public ListboxItem this[int index] {
    get {
      return (ListboxItem)list[index];
    }

    set {
      list[index] = value;
    }
  }

}
#endregion

#region GridRow Collection

public class GridRowList {
  ArrayList list = new ArrayList();

  public int Find(Widgets.GridRow row) {
    return list.IndexOf(row);
  }

  public void Add(Widgets.GridRow row) {
    list.Add(row);
  }

  public void Clear() {
    list.Clear();
  }

  public void Insert(int position, Widgets.GridRow row) {
    if(position >= list.Count) {
      list.Add(row);
    } else {
      list.Insert(position, row);
    }

  }

  public void Remove(Widgets.GridRow row) {
    list.Remove(row);
  }

  public void RemoveAt(int index) {
    list.RemoveAt(index);
  }

  public void SortAscending() {
    list.Sort(new GridRowLTComparer());
  }

  public void SortDescending() {
    list.Sort(new GridRowGTComparer());
  }

  public int Count {
    get {
      return list.Count;
    }
  }

  public Widgets.GridRow this[int index] {
    get {
      return (Widgets.GridRow)list[index];
    }
  }

}
#endregion

#region ListHeaderSegment collection

public class HeaderSegmentList {
  ArrayList list = new ArrayList();

  public int Find(Widgets.ListHeaderSegment item) {
    return list.IndexOf(item);
  }

  public void Add(Widgets.ListHeaderSegment item) {
    list.Add(item);
  }

  public void Clear() {
    list.Clear();
  }

  public void Insert(int position, Widgets.ListHeaderSegment item) {
    if(position >= list.Count) {
      list.Add(item);
    } else {
      list.Insert(position, item);
    }
  }

  public void Remove(Widgets.ListHeaderSegment item) {
    list.Remove(item);
  }

  public int Count {
    get {
      return list.Count;
    }
  }

  public Widgets.ListHeaderSegment this[int index] {
    get {
      return (Widgets.ListHeaderSegment)list[index];
    }
  }

}
#endregion


#region StringDictionary
public class StringDictionary : DictionaryBase {
  public string this[string key] {
    get {
      return (string)Dictionary[key];
    }
    set {
      Dictionary[key] = value;
    }
  }

  public ICollection Keys {
    get {
      return Dictionary.Keys;
    }
  }

  public ICollection Values {
    get {
      return Dictionary.Values;
    }
  }

  public void Add(string key, string val) {
    Dictionary.Add(key, val);
  }

  public bool Contains(string key) {
    return Dictionary.Contains(key);
  }

  public void Remove(string key) {
    Dictionary.Remove(key);
  }
}
#endregion StringDictionary

#region Comparers for GridRow Sorting

/// <summary>
///		Greater-than Comparer used for GridRows
/// </summary>
public class GridRowGTComparer : System.Collections.IComparer {
  #region IComparer Members

  public int Compare(object x, object y) {
    Widgets.GridRow itemA = (Widgets.GridRow)x;
    Widgets.GridRow itemB = (Widgets.GridRow)y;

    if(itemA < itemB) {
      return -1;
    } else if(itemA > itemB) {
      return 1;
    }
      // items are equal
  else {
      return 0;
    }
  }

  #endregion
}

/// <summary>
///		Less-than Comparer used for GridRows
/// </summary>
public class GridRowLTComparer : System.Collections.IComparer {
  #region IComparer Members

  public int Compare(object x, object y) {
    return (new GridRowGTComparer()).Compare(y, x);
  }

  #endregion
}

#endregion

} // namespace CeGui