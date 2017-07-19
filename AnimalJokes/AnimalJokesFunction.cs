using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.Core;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Newtonsoft.Json;
using AnimalJokes.Data;
using System.Xml.Linq;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AnimalJokes
{
    public class Function
    {
        public List<JokeResource> GetLocaleResources()
        {
            List<JokeResource> resources = new List<JokeResource>();

            JokeResource enGBResource = new JokeResource("en-GB");

            GetMessgesForResource(enGBResource);
            GetJokesForResource(enGBResource);

            resources.Add(enGBResource);
            return resources;
        }

        private void GetMessgesForResource(JokeResource enGBResource)
        {
            enGBResource.SkillName = "Animal Jokes";
            enGBResource.GetJokeMessage = "Here's your animal joke: ";
            enGBResource.HelpMessage = "You can say tell me an animal joke, or, you can say exit... What can I help you with?";
            enGBResource.HelpReprompt = "You can say tell me an animal joke to start";
            enGBResource.StopMessage = "Goodbye! Come back for a new joke any time";
        }

        public void GetJokesForResource(JokeResource resource)
        {
            XDocument jokesDoc = XDocument.Load("Jokes.xml");

            if (jokesDoc == null)
            {
                return;
            }

            XElement jokesRootElement = jokesDoc.Root;

            IEnumerable<XElement> jokes = jokesRootElement.Elements("Joke");

            if (jokes.Any())
            {
                foreach (XElement joke in jokes)
                {
                    Joke jokeFromXmlDoc = new Joke
                    {
                        JokeText = joke.Element("JokeText").Value
                    };

                    resource.Jokes.Add(jokeFromXmlDoc);
                }
            }
        }

        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            SkillResponse response = new SkillResponse();
            response.Response = new ResponseBody();
            response.Response.ShouldEndSession = false;
            IOutputSpeech innerResponse = null;
            ILambdaLogger log = context.Logger;
            log.LogLine($"Skill Request Object:");
            log.LogLine(JsonConvert.SerializeObject(input));

            List<JokeResource> allResources = GetLocaleResources();
            JokeResource resource = allResources.FirstOrDefault();

            if (input.GetRequestType() == typeof(LaunchRequest))
            {
                log.LogLine($"Default LaunchRequest made: 'Alexa, open Animal Jokes");
                innerResponse = new PlainTextOutputSpeech();
                (innerResponse as PlainTextOutputSpeech).Text = EmitNewJoke(resource, withLaunchPreface: true);

            }
            else if (input.GetRequestType() == typeof(IntentRequest))
            {
                IntentRequest intentRequest = (IntentRequest)input.Request;

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
                        (innerResponse as PlainTextOutputSpeech).Text = EmitNewJoke(resource, false);
                        response.Response.ShouldEndSession = true;
                        break;

                    case "GetNewJokeIntent":
                        log.LogLine($"GetNewJokeIntent sent: send new joke");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = EmitNewJoke(resource, false);
                        response.Response.ShouldEndSession = true;
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

        public string EmitNewJoke(JokeResource resource, bool withLaunchPreface)
        {
            Random r = new Random();

            if (withLaunchPreface)
            {
                return resource.GetJokeMessage + resource.Jokes[r.Next(resource.Jokes.Count)].JokeText;
            }

            return resource.Jokes[r.Next(resource.Jokes.Count)].JokeText;
        }
    }
}
