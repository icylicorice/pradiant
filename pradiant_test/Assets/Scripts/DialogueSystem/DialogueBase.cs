using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DialogueSystem
{

    public class DialogueBase : MonoBehaviour
    {
        public bool finished { get; private set; }
        protected IEnumerator WriteText(string input, Text textholder, Color textColor, float delay)
        {
            textholder.color = textColor;
            for (int i = 0; i < input.Length; i++)
            {
                textholder.text += input[i];
                yield return new WaitForSeconds(delay);
            }

            yield return new WaitUntil(() => Input.GetMouseButton(0));
            finished = true;
        }
    }
}
