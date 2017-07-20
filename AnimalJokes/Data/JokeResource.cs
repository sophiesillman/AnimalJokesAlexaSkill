using System.Collections.Generic;

namespace AnimalJokes.Data
{
    public class JokeResource
    {
        public JokeResource(string language)
        {
            Language = language;
            Jokes = new List<Joke>();
        }

        public string Language { get; set; }
        public string SkillName { get; set; }
        public List<Joke> Jokes { get; set; }
        public string HelpMessage { get; set; }
        public string HelpReprompt { get; set; }
        public string RequestAnotherJokePrompt { get; set; }
        public string StopMessage { get; set; }
    }
}
