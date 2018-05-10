using Quantum;
using UnityEngine;
using UnityEditor;
namespace Quantum.Editor {

[CustomPropertyDrawer(typeof(CharacterSpecLink))]
public class CharacterSpecLinkPropertyDrawer : PropertyDrawer {
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    AssetLinkDrawer.DrawAssetObjectSelector(position, property, label, typeof(CharacterSpecAsset));
  }
}

}
