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
            enGBResource.HelpMessage = "You can say tell me an animal joke, or, you can say exit... What can I help you with?";
            enGBResource.HelpReprompt = "You can say tell me an animal joke to start";
            enGBResource.StopMessage = "Goodbye! Come back for a joke soon";
            enGBResource.RequestAnotherJokePrompt = "Would you like to hear another joke?";
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
            ILambdaLogger log = context.Logger;
            log.LogLine($"Skill Request Object:");
            log.LogLine(JsonConvert.SerializeObject(input));

            SsmlOutputSpeech innerResponse = new SsmlOutputSpeech();

            List<JokeResource> allResources = GetLocaleResources();
            JokeResource resource = allResources.FirstOrDefault();

            if (input.GetRequestType() == typeof(LaunchRequest))
            {
                log.LogLine($"Default LaunchRequest made: 'Alexa, open Animal Jokes");
                innerResponse.Ssml = EmitNewJoke(resource, withLaunchPreface: true, isSsml: true);
            }
            else if (input.GetRequestType() == typeof(IntentRequest))
            {
                IntentRequest intentRequest = (IntentRequest)input.Request;

                string intentName = intentRequest.Intent.Name;

                switch (intentName)
                {
                    case "AMAZON.CancelIntent":
                        log.LogLine($"AMAZON.CancelIntent: send StopMessage");
                        innerResponse.Ssml = "<speak>" + resource.StopMessage + "</speak>";
                        response.Response.ShouldEndSession = true;
                        break;

                    case "AMAZON.StopIntent":
                        log.LogLine($"AMAZON.StopIntent: send StopMessage");
                        innerResponse.Ssml = "<speak>" + resource.StopMessage + "</speak>";
                        response.Response.ShouldEndSession = true;
                        break;

                    case "AMAZON.HelpIntent":
                        log.LogLine($"AMAZON.HelpIntent: send HelpMessage");
                        innerResponse.Ssml = "<speak>" + resource.HelpMessage + "</speak>";
                        break;

                    case "GetJokeIntent":
                        log.LogLine($"GetJokeIntent sent: send new joke");
                        innerResponse.Ssml = EmitNewJoke(resource, false, false);
                        response.Response.ShouldEndSession = true;
                        break;

                    case "GetNewJokeIntent":
                        log.LogLine($"GetNewJokeIntent sent: send new joke");
                        innerResponse.Ssml = EmitNewJoke(resource, false, false);
                        response.Response.ShouldEndSession = true;
                        break;

                    default:
                        log.LogLine($"Unknown intent: " + intentRequest.Intent.Name);
                        innerResponse.Ssml = "<speak>" + resource.HelpReprompt + "</speak>";
                        break;
                }
            }

            response.Response.OutputSpeech = innerResponse;
            response.Version = "1.0";
            log.LogLine($"Skill Response Object...");
            log.LogLine(JsonConvert.SerializeObject(response));
            return response;
        }

        public string EmitNewJoke(JokeResource resource, bool withLaunchPreface, bool isSsml)
        {
            Random r = new Random();

            string responseString = "<speak>";

            if (withLaunchPreface && isSsml)
            {
                responseString += resource.Jokes[r.Next(resource.Jokes.Count)].JokeText + "<break time='1s' />" + resource.RequestAnotherJokePrompt;
            }
            else
            {
                responseString += resource.Jokes[r.Next(resource.Jokes.Count)].JokeText;
            }

            return responseString + "</speak>";
        }
    }
}
