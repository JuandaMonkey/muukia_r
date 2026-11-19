using System;
using System.Collections.Generic;
using UnityEngine;

namespace MUUKIA
{
    [Serializable]
    public class DialogueLine
    {
        public string speakerName;
        [TextArea(2, 6)]
        public string text;
        public AudioClip voiceAudioClip;
        public float typewriterSpeed = 0.035f;
        public bool isOffscreen = false;
        public bool autoAdvance = false;
        public float autoAdvanceDelay = 2.5f;
    }

    [CreateAssetMenu(fileName = "NewDialogueSequence", menuName = "MUUKIA/Dialogue Sequence")]
    public class DialogueSequenceAsset : ScriptableObject
    {
        public string sequenceId;
        public List<DialogueLine> lines = new List<DialogueLine>();
    }
}
