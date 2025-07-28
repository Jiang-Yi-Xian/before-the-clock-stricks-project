using System;
using System.Collections.Generic;

[Serializable]
public class SubtitleLine
{
    public string text;
    public float time;
}

[Serializable]
public class SubtitleData
{
    public List<SubtitleLine> lines;

    public float timePerCharacter = 0.2f;
    public float sentenceEndDelay = 0.5f;
}