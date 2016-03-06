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

namespace CeGui {

/// <summary>
/// Summary description for TextUtil.
/// </summary>
public class TextUtil {
  #region Constants

  /// <summary>
  ///		The default set of whitespace.
  /// </summary>
  public const string DefaultWhitespace = " \n\t\r";
  /// <summary>
  ///		The default set of alpha-numeric characters.
  /// </summary>
  public const string DefaultAlphanumerical = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
  /// <summary>
  ///		The default set of word-wrap delimiters.
  /// </summary>
  public const string DefaultWrapDelimiters = " \n\t\r";

  #endregion Constants

  /// <summary>
  ///		Private: meant to be a static class.
  /// </summary>
  private TextUtil() { }

  #region Static Methods

  /// <summary>
  ///		Returns a String containing the the next word in a String.
  /// </summary>
  /// <remarks>
  ///		This method returns a String containing the word, starting at <paramref name="startIndex"/>, 
  ///		of <paramref name="text"/> as delimited by the characters specified in <paramref name="delimeters"/> 
  ///		(or the ends of the input string).
  /// </remarks>
  /// <param name="text">Text to test.</param>
  /// <param name="startIndex">Index into <paramref name="text"/> where the search for the next word is to begin.</param>
  /// <param name="delimeters">
  ///		String containing the set of delimiter code points to be used when determining the start and end
  ///		points of a word in <paramref name="text"/>.</param>
  /// <returns>
  ///		String object containing the next <paramref name="delimeters"/> delimited word from 
  ///		<paramref name="text"/>, starting at index <paramref name="startIndex"/>.
  ///	</returns>
  public static string GetNextWord(string text, int startIndex, string delimeters) {
    int wordStart = IndexNotOf(text, delimeters, startIndex);

    if(wordStart == -1) {
      wordStart = startIndex;
    }

    int wordEnd = text.IndexOfAny(delimeters.ToCharArray(), wordStart);

    if(wordEnd == -1) {
      wordEnd = text.Length;
    }

    return text.Substring(startIndex, (wordEnd - startIndex));
  }

  public static string GetNextWord(string text, int startIndex) {
    return GetNextWord(text, startIndex, DefaultWhitespace);
  }

  public static string GetNextWord(string text) {
    return GetNextWord(text, 0, DefaultWhitespace);
  }

  /// <summary>
  ///		Return the index of the first character of the word after the word at 'index'.
  /// </summary>
  /// <param name="str">String containing text.</param>
  /// <param name="index">Index into 'str' where search is to begin.</param>
  /// <returns>
  ///		Index into 'str' which marks the begining of the word at after the word at 'index'.
  ///		If 'index' is within the last word, then the return is the last index in 'str'.
  /// </returns>
  public static int GetNextWordStartIdx(string str, int index) {
    int length = str.Length;

    // do some checks for simple cases
    if((index >= length) || (length == 0)) {
      return length;
    }

    // is character at 'idx' alphanumeric
    if(-1 != DefaultAlphanumerical.IndexOf(str[index])) {
      // find position of next character that is not alphanumeric
      index = IndexNotOf(str, DefaultAlphanumerical, index);
    }
      // is character also not whitespace (therefore a symbol)
  else if(-1 == DefaultWhitespace.IndexOf(str[index])) {
      // find index of next character that is either alphanumeric or whitespace
      index = str.LastIndexOf(DefaultAlphanumerical + DefaultWhitespace, index);
    }

    // check result at this stage.
    if(-1 == index) {
      index = length;
    } else {
      // if character at 'idx' is whitespace
      if(-1 != DefaultWhitespace.IndexOf(str[index])) {
        // find next character that is not whitespace
        index = IndexNotOf(str, DefaultWhitespace, index);
      }

      if(-1 == index) {
        index = length;
      }
    }

    return index;
  }

  /// <summary>
  ///		Return the index of the first character of the word at 'index'.
  /// </summary>
  /// <param name="str">String containing text.</param>
  /// <param name="index">Index into 'str' were search of work is to begin.</param>
  /// <returns>Index into 'str' which marks the beginning of the work at 'index'.</returns>
  public static int GetWordStartIdx(string str, int index) {
    string temp = str.Substring(0, index);

    temp = temp.TrimEnd(DefaultWhitespace.ToCharArray());

    if(temp.Length <= 1) {
      return 0;
    }

    // identify the type of character at 'pos'
    if(-1 != DefaultAlphanumerical.IndexOf(temp[temp.Length - 1])) {
      index = LastIndexNotOf(temp, DefaultAlphanumerical, 0);
    }
      // since whitespace was stripped, character must be a symbol
  else {
      index = temp.LastIndexOfAny((DefaultAlphanumerical + DefaultWhitespace).ToCharArray());
    }

    // make sure we do not go past end of string (+1)
    if(index == -1) {
      return 0;
    } else {
      return index + 1;
    }
  }

  /// <summary>
  ///		
  /// </summary>
  /// <param name="delimiters"></param>
  /// <param name="startIndex"></param>
  /// <returns></returns>
  public static int IndexNotOf(string text, string delimiters, int startIndex) {
    int index = startIndex;

    while(index < text.Length) {
      if(delimiters.IndexOf(text[index]) == -1) {
        return index;
      }

      index++;
    }

    return -1;
  }

  public static int LastIndexNotOf(string text, string delimiters, int startIndex) {
    int index = startIndex;
    int foundIdx = -1;

    while(index < text.Length) {
      if(delimiters.IndexOf(text[index]) == -1) {
        foundIdx = index;
      }

      index++;
    }

    return foundIdx;
  }

  #endregion Static Methods
}

} // namespace CeGui
