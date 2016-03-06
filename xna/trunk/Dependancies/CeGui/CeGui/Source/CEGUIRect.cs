//
// Original files:
//   include/CEGUIRect.h
//   src/CEGUIRect.cpp
//
// Ported from Revision:
//   1248
//
// Notes:
//   (cygon)
//     It would be nice if this class could be entirely replaced by System.Drawing.RectangleF.
//     However, the left, top, right and bottom properties of the RectangleF class are not
//     assignable, making the ported code look less like the original. So as a compromise,
//     I've added implicit conversion operators to and from System.Drawing.RectangleF that
//     should make it just as easy.
//
