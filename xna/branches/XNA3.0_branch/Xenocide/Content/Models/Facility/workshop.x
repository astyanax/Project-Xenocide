xof 0303txt 0032

Header {
 1;
 0;
 1;
}

Frame SceneRoot {
  FrameTransformMatrix {
    1.00000,0.00000,0.00000,0.00000,
    0.00000,1.00000,0.00000,0.00000,
    0.00000,0.00000,1.00000,0.00000,
    0.00000,0.00000,0.00000,1.00000;;
  }

  Mesh Box {
   4;
   0.500000;0.000000;-0.500000;,
   -0.500000;0.000000;-0.500000;,
   -0.500000;0.000000;0.500000;,
   0.500000;0.000000;0.500000;;
   2;
   3;0,1,2;,
   3;0,2,3;;

   MeshMaterialList {
    1;
    2;
    0,
    0;

    // Embed the texture
    Material textured {
      1.000000;1.000000;1.000000;1.000000;;
      0.000000;
      0.000000;0.000000;0.000000;;
      0.000000;0.000000;0.000000;;
      TextureFilename {    "FacilityNames.PNG";  }
    }
   }

   MeshNormals {
    1;
    0.000000;1.000000;0.000000;;
    2;
    3;0,0,0;,
    3;0,0,0;;
   }

   MeshTextureCoords {
    4;
    0.780392;0.584314;,
    0.592157;0.584314;,
    0.592157;0.396078;,
    0.780392;0.396078;;
   }
  }
}