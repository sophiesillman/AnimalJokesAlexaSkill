using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.Core;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AnimalJokes
{
    public class Function
    {
        public List<JokeResource> GetResources()
        {
            List<JokeResource> resources = new List<JokeResource>();

            JokeResource enGBResource = new JokeResource("en-GB");
            enGBResource.SkillName = "Animal Jokes";
            enGBResource.GetJokeMessage = "Here's your animal joke: ";
            enGBResource.HelpMessage = "You can say tell me an animal joke, or, you can say exit... What can I help you with?";
            enGBResource.HelpReprompt = "You can say tell me an animal joke to start";
            enGBResource.StopMessage = "Goodbye!";
            enGBResource.Jokes.Add("What did the duck say when he bought lipstick? Put it on my bill!");
            enGBResource.Jokes.Add("Why couldn't the leopard play hide and seek? Because he was always spotted!");
            enGBResource.Jokes.Add("What do you call a pig that does Karate? A pork chop");
            enGBResource.Jokes.Add("What's the difference between a guitar and a fish? You can tune a guitar, but you can't tuna fish");
            enGBResource.Jokes.Add("Why did the fish blush? Because it saw the ocean's bottom");
            enGBResource.Jokes.Add("Why do the french eat snails? They don't like fast food!");
            enGBResource.Jokes.Add("What type of sandals do frogs wear? Open toad!");
            enGBResource.Jokes.Add("What do you get from a pampered cow? Spoiled milk");
            enGBResource.Jokes.Add("What do you get when you cross a snake and a bakery? A python!");
            enGBResource.Jokes.Add("What is out of bounds? An exhausted kangaroo");
            enGBResource.Jokes.Add("What do you call a bear with no ears? A b!");

            resources.Add(enGBResource);
            return resources;
        }

        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            SkillResponse response = new SkillResponse();
            response.Response = new ResponseBody();
            response.Response.ShouldEndSession = false;
            IOutputSpeech innerResponse = null;
            var log = context.Logger;
            log.LogLine($"Skill Request Object:");
            log.LogLine(JsonConvert.SerializeObject(input));

            List<JokeResource> allResources = GetResources();
            JokeResource resource = allResources.FirstOrDefault();

            if (input.GetRequestType() == typeof(LaunchRequest))
            {
                log.LogLine($"Default LaunchRequest made: 'Alexa, open Animal Jokes");
                innerResponse = new PlainTextOutputSpeech();
                (innerResponse as PlainTextOutputSpeech).Text = emitNewJoke(resource, withLaunchPreface: true);

            }
            else if (input.GetRequestType() == typeof(IntentRequest))
            {
                var intentRequest = (IntentRequest)input.Request;

                string intentName = intentRequest.Intent.Name;

                switch (intentName)
                {
                    case "AMAZON.CancelIntent":
                        log.LogLine($"AMAZON.CancelIntent: send StopMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.StopMessage;
                        response.Response.ShouldEndSession = true;
                        break;

                    case "AMAZON.StopIntent":
                        log.LogLine($"AMAZON.StopIntent: send StopMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.StopMessage;
                        response.Response.ShouldEndSession = true;
                        break;

                    case "AMAZON.HelpIntent":
                        log.LogLine($"AMAZON.HelpIntent: send HelpMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.HelpMessage;
                        break;

                    case "GetJokeIntent":
                        log.LogLine($"GetJokeIntent sent: send new joke");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = emitNewJoke(resource, false);
                        break;

                    case "GetNewJokeIntent":
                        log.LogLine($"GetNewJokeIntent sent: send new joke");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = emitNewJoke(resource, false);
                        break;

                    default:
                        log.LogLine($"Unknown intent: " + intentRequest.Intent.Name);
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.HelpReprompt;
                        break;
                }
            }

            response.Response.OutputSpeech = innerResponse;
            response.Version = "1.0";
            log.LogLine($"Skill Response Object...");
            log.LogLine(JsonConvert.SerializeObject(response));
            return response;
        }

        public string emitNewJoke(JokeResource resource, bool withLaunchPreface)
        {
            Random r = new Random();

            if (withLaunchPreface)
            {
                return resource.GetJokeMessage + resource.Jokes[r.Next(resource.Jokes.Count)];
            }

            return resource.Jokes[r.Next(resource.Jokes.Count)];
        }
    }
        
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
