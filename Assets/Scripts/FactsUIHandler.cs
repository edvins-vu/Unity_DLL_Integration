using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace Assets.Scripts
{
    public class FactUIHandler : MonoBehaviour
    {
        public TextMeshProUGUI responseText;


        // Add parsed response text to game display object
        public void UpdateFacts(List<Fact> facts)
        {
            string formattedText = "Facts:\n";

            foreach (Fact fact in facts)
            {
                formattedText += $"- {fact.attributes.body}\n";
            }

            responseText.text = formattedText;
        }
    }
}