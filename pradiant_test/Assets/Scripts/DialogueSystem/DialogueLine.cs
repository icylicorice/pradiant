using System.Collections;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DialogueSystem
{
    public class DialogueLine : DialogueBase
    {
        private Text textHolder;

        [Header("Text Options")]
        [SerializeField] private string input;
        [SerializeField] private Color textColor;

        [Header("Time parameter")]
        [SerializeField] private float delay;
        private void Awake()
        {
            textHolder = GetComponent<Text>();
            textHolder.text = "";
        }
        private void OnEnable()
        {
            StartCoroutine(WriteText(input, textHolder, textColor, delay));
        }
    }
}