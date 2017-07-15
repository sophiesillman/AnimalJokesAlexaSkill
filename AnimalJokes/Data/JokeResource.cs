using System.Collections.Generic;

namespace AnimalJokes.Data
{
    public class JokeResource
    {
        public JokeResource(string language)
        {
            Language = language;
            Jokes = new List<string>();
        }

        public string Language { get; set; }
        public string SkillName { get; set; }
        public List<string> Jokes { get; set; }
        public string GetJokeMessage { get; set; }
        public string HelpMessage { get; set; }
        public string HelpReprompt { get; set; }
        public string StopMessage { get; set; }
    }
}
