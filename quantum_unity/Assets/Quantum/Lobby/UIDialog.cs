using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI = UnityEngine.UI;

namespace Quantum.Example {
  public class UIDialog : UIScreen<UIDialog> {
    public UI.Text Text;

    public static void Show(String text, params System.Object[] args) {
      // set text
      Instance.Text.text = String.Format(text, args);

      // show screen
      ShowScreen();
    }
  }
}