using Photon.Deterministic;
using UnityEditor;
using UnityEngine;

namespace Quantum {
  public static class GenerateLookUpTables {
    [MenuItem("Quantum/Generate Math Lookup Tables")]
    public static void Generate() {
      var sep = System.IO.Path.DirectorySeparatorChar;
      FPLut.GenerateTables(Application.dataPath + sep + "Quantum" + sep + "Resources" + sep + "LUT");

      // this makes sure the tables are loaded into unity
      AssetDatabase.Refresh();
    }
  }
}