using Newtonsoft.Json;
using SharpVectors.Converters;
using SharpVectors.Dom;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HS
{
    /// <summary>
    /// Interaction logic for View.xaml
    /// </summary>
    public partial class ViewCode : Window
    {
        // NOT IN USE IN v0.1
        dynamic globalJSON = null;

        public ViewCode()
        {
            InitializeComponent();
            projectViewerHomePage();
        }

        List<dynamic> projectData = new List<dynamic>();

        public class block
        {
            public string colour = "#cd1c10";
            public string a = @"<div style=""margin-bottom: 25px; box-shadow: inset -15px 0 0 #d1d1d1, inset 0 -15px 0 #d1d1d1, inset 15px 0 0 #d1d1d1, inset 0 15px 0 #d1d1d1; background-color:#f1f1f1; min-height: 20px; padding: 25px;""><div style=""background-color:#d1d1d1; width: calc(100% + 50px); margin-left:-25px; margin-top:-25px; margin-bottom: 30px; height: 50px;""><div style=""padding: 15px; font-size: 20px; font-family: arial;"">";
            public string text = "";
            public string c = @"</div></div>";
            public List<block> blocks = new List<block>();
            public string end = @"</div>";
        }

        public string getNameFromObjectParameterTypeKey(dynamic id)
        {
            id = Convert.ToString(id);
            switch (id)
            {
                case "8001":
                    return "Anything";
                case "8002":
                    return "Edge";
                case "8003":
                    return "Game";
                case "8004":
                    return "Self";
                case "8005":
                    return "Self";
                case "8006":
                    return "Local";
                case "8007":
                    return "User";
                default:
                    return "";
            }
        }

        public string getObjectName(string id)
        {
            var objects = globalJSON.objects;
            foreach (var obj in objects)
            {
                if (obj.objectID == id)
                {
                    return obj.name;
                }
            }
            return "Object Not Found";
        }

        public string getParamName(dynamic input, bool includeParamHTML = true) // send one part of param array only
        {
            string first_part_of_span = "";
            string second_part_of_span = "";
            if (includeParamHTML)
            {
                first_part_of_span = "<span style=\"border-radius:50px; padding:3px 6px 3px 6px; background-color:#f0f0f0;\">";
                second_part_of_span = "</span>";
            }

            if (input.ContainsKey("datum") == true && input.datum.ContainsKey("HSTraitIDKey") == false) // That means its either a variable or a colour
            {
                if (input.datum.ContainsKey("type") && (Convert.ToString(input.datum.type) == "5000" || Convert.ToString(input.datum.type) == "5001" || Convert.ToString(input.datum.type) == "5002"))
                {
                    return first_part_of_span + Convert.ToString(input.value) + second_part_of_span;
                }
                else if (input.datum.ContainsKey("type") && ((input.datum.type >= 4000 && input.datum.type <= 4009) || (input.datum.type >= 1000 && input.datum.type <= 1010))) // math operator or a conditional
                {
                    return getParamName(input.datum[@"params"][0]) + " " + input.datum.description + " " + getParamName(input.datum[@"params"][1]);
                }
                else
                {
                    string variable_id = Convert.ToString(input.datum.variable);
                    dynamic variables = globalJSON.variables;
                    foreach (var variable in variables)
                    {
                        if (variable.objectIdString == variable_id)
                        {
                            return first_part_of_span + variable.name + second_part_of_span;
                        }
                    }
                    return first_part_of_span + "datum found but variable not found" + second_part_of_span;
                }
            }

            else if (input.ContainsKey("datum") == true && input.datum.ContainsKey("HSTraitIDKey") == true) // trait
            {
                string traitname = "";
                if (Convert.ToString(input.datum.HSTraitObjectParameterTypeKey) == "8000")
                {
                    traitname = getObjectName(Convert.ToString(input.datum.HSTraitObjectIDKey));
                }
                else {
                    traitname = getNameFromObjectParameterTypeKey(input.datum.HSTraitObjectParameterTypeKey);
                }

                if (input.datum.HSTraitTypeKey >= 3000 && input.datum.HSTraitTypeKey < 4000) //game variable (return with play symbol instead)
                {
                    return first_part_of_span + "▶️ " + input.datum.description + second_part_of_span;
                }
                else
                {
                    return first_part_of_span + "(" + traitname + ") " + input.datum.description + second_part_of_span;
                }
            }

            else if (Convert.ToString(input.type) == "42" || Convert.ToString(input.type) == "43" || Convert.ToString(input.type) == "48" || Convert.ToString(input.type) == "57") // just plain old number
            {
                return first_part_of_span + input.value + second_part_of_span;
            }

            else // then it must be either an object or something like "ipad"
            {
                dynamic eventParameters = globalJSON.eventParameters;
                foreach (var eventParameter in eventParameters)
                {
                    if (Convert.ToString(eventParameter.id) == Convert.ToString(input.variable))
                    {
                        // if it is not an object, we can just return the description, otherwise we must return the object's name as the description will be "object"
                        if (Convert.ToString(eventParameter.blockType) == "8000")
                        {
                            dynamic objects = globalJSON.objects;
                            foreach (var obj in objects)
                            {
                                if (Convert.ToString(obj.objectID) == Convert.ToString(eventParameter.objectID))
                                {
                                    return first_part_of_span + obj.name + second_part_of_span;
                                }
                            }
                            return first_part_of_span + "identified as object but object not found" + second_part_of_span;

                        }
                        else // not an object -> return description
                        {
                            return first_part_of_span + eventParameter.description + second_part_of_span;
                        }
                    }
                }
            }
            return first_part_of_span + "not found" + second_part_of_span;
        }

        public void display(object sender, RoutedEventArgs e)
        {
            string htmlOutput = "";
            foreach (dynamic a in projectData)
            {
                if (Convert.ToString(a.generalType) == "SCENE")
                {
                    block sceneElement = new block() { text = a.name, colour = "#d1d1d1" };

                    foreach (dynamic b in projectData)
                    {
                        if (Convert.ToString(b.generalType) == "OBJECT" && b.parentID == a.myID)
                        {
                            block objectElement = new block() { text = b.name, colour = "#6bdaed" };
                            sceneElement.blocks.Add(objectElement);

                            foreach (dynamic c in projectData)
                            {
                                if (Convert.ToString(c.generalType) == "RULE" && c.parentID == b.myID)
                                {

                                    // Technically the only rule as of now is "WHEN" as all other rules are the children of "WHEN" e.g. "When Game Starts", "When Tapped", "When 7==7"
                                    block ruleElement = new block() { text = "When ", colour = "#be2961" };

                                    // So I'm just going to skip the switch for rules and jump right to events (children of rules) like "Game Starts"
                                    switch (Convert.ToString(c.parameters[0].datum.type))
                                    {
                                        case "7000":
                                            ruleElement.text += "Game Starts";
                                            break;
                                        case "7001":
                                            ruleElement.text += getParamName(c.parameters[0].datum[@"params"][0]) + " Is Tapped";
                                            break;
                                        case "7002":
                                            ruleElement.text += getParamName(c.parameters[0].datum[@"params"][0]) + " Is Touching " + getParamName(c.parameters[0].datum[@"params"][1]);
                                            break;
                                        case "7003":
                                            ruleElement.text += getParamName(c.parameters[0].datum[@"params"][0]) + " Is Pressed";
                                            break;
                                        case "7004":
                                            ruleElement.text += "Device Is Tilted Right";
                                            break;
                                        case "7005":
                                            ruleElement.text += "Device Is Tilted Left";
                                            break;
                                        case "7006":
                                            ruleElement.text += "Device Is Tilted Up";
                                            break;
                                        case "7007":
                                            ruleElement.text += "Device Is Tilted Down";
                                            break;
                                        case "7008":
                                            ruleElement.text += "Device Hears A Loud Noise";
                                            break;
                                        case "7009":
                                            ruleElement.text += "Device Is Shaken";
                                            break;
                                        case "7010":
                                            ruleElement.text += getParamName(c.parameters[0].datum[@"params"][0]) + " Bumps " + getParamName(c.parameters[0].datum[@"params"][1]);
                                            break;
                                        case "7011":
                                            ruleElement.text += getParamName(c.parameters[0].datum[@"params"][0]) + " Is Swiped Right";
                                            break;
                                        case "7012":
                                            ruleElement.text += getParamName(c.parameters[0].datum[@"params"][0]) + " Is Swiped Left";
                                            break;
                                        case "7013":
                                            ruleElement.text += getParamName(c.parameters[0].datum[@"params"][0]) + " Is Swiped Up";
                                            break;
                                        case "7014":
                                            ruleElement.text += getParamName(c.parameters[0].datum[@"params"][0]) + " Is Swiped Down";
                                            break;
                                        case "7015":
                                            ruleElement.text += "Object Is Cloned";
                                            break;
                                        case "7020":
                                            ruleElement.text += getParamName(c.parameters[0].datum[@"params"][0]) + " Is Not Pressed";
                                            break;
                                        case "7021":
                                            ruleElement.text += "Game Is Playing";
                                            break;
                                        case "7023":
                                            ruleElement.text += "I Get A Message " + getParamName(c.parameters[0].datum[@"params"][0]);
                                            break;
                                        case "7024":
                                            ruleElement.text += "Message Matches " + getParamName(c.parameters[0].datum[@"params"][0]);
                                            break;
                                        case "7025":
                                            ruleElement.text += getParamName(c.parameters[0].datum[@"params"][0]) + " Is Not Touching " + getParamName(c.parameters[0].datum[@"params"][1]);
                                            break;
                                        case "1000": // equal
                                            ruleElement.text += getParamName(c.parameters[0].datum[@"params"][0]) + " = " + getParamName(c.parameters[0].datum[@"params"][1]);
                                            break;
                                        case "1001": // not equal
                                            ruleElement.text += getParamName(c.parameters[0].datum[@"params"][0]) + " &#8800; " + getParamName(c.parameters[0].datum[@"params"][1]);
                                            break;
                                        case "1002": // less than
                                            ruleElement.text += getParamName(c.parameters[0].datum[@"params"][0]) + " < " + getParamName(c.parameters[0].datum[@"params"][1]);
                                            break;
                                        case "1003": // more than
                                            ruleElement.text += getParamName(c.parameters[0].datum[@"params"][0]) + " > " + getParamName(c.parameters[0].datum[@"params"][1]);
                                            break;
                                        case "1004": // and
                                            ruleElement.text += generateEventString(c.parameters[0].datum,"and");
                                            break;



                                        default:
                                            ruleElement.text = c.parameters[0].datum.description; // if type unknown
                                            break;
                                    }
                                    objectElement.blocks.Add(ruleElement);

                                    ruleElement = addblock(ruleElement, c);
                                }
                            }
                        }
                    }

                    string temp_firstpart = sceneElement.a;
                    temp_firstpart = Regex.Replace(temp_firstpart, @"#d1d1d1", sceneElement.colour);
                    htmlOutput += temp_firstpart + sceneElement.text + sceneElement.c;
                    htmlOutput += stepIn(sceneElement);
                    htmlOutput += sceneElement.end;
                    //MessageBox.Show(htmlOutput);
                }
            }

            projectViewer.NavigateToString(htmlOutput);
        }

        public string generateEventString(dynamic datum, string seperator) // only accepts datum
        {
            if (datum == null)
            {
                return "";
            }
            string left_side = "";
            string right_side = "";
            if (datum.ContainsKey("params") == true)
            {
                var snuffy = datum; 
                if (datum[@"params"][0].ContainsKey("datum"))
                {
                    left_side = generateEventString(datum[@"params"][0].datum, Convert.ToString(datum[@"params"][0].datum.description));
                }
                else
                {
                    left_side = getParamName(datum[@"params"][0]);
                }

                if (datum[@"params"][1].ContainsKey("datum"))
                {
                    right_side = generateEventString(datum[@"params"][1].datum, Convert.ToString(datum[@"params"][1].datum.description));
                }
                else
                {
                    right_side = getParamName(datum[@"params"][1]);
                }
            } else
            {
                left_side = getParamName(datum);
                right_side = getParamName(datum);
            }
            return left_side + " " + seperator + " " + right_side;
        }

        public block addblock(block ruleElement, dynamic rule)
        {
            foreach (dynamic a in projectData)
            {
                if (Convert.ToString(a.generalType) == "BLOCK" && a.parentID == rule.myID)
                {
                    string colour = "#d4d4d4";
                    string text = a.description; //default

                    switch (Convert.ToString(a.type))
                    {
                        case "33": //change pose
                            colour = "#63ae1e";
                            text = "Change Pose";
                            break;
                        case "40": //set text
                            colour = "#63ae1e";
                            text = "Set Text " + getParamName(a.parameters[0]) + " colour " + getParamName(a.parameters[1]);
                            break;
                        case "42": //send to back
                            colour = "#63ae1e";
                            break;
                        case "43": //bring to front
                            colour = "#63ae1e";
                            break;
                        case "47": //set invisibility
                            colour = "#63ae1e";
                            text = "Set Invisibility percent " + getParamName(a.parameters[0]);
                            break;
                        case "48": //grow by
                            colour = "#63ae1e";
                            text = "Grow By percent " + getParamName(a.parameters[0]);
                            break;
                        case "49": //shrink by
                            colour = "#63ae1e";
                            text = "Shrink By percent " + getParamName(a.parameters[0]);
                            break;
                        case "51": //set size
                            colour = "#63ae1e";
                            text = "Set Size percent " + getParamName(a.parameters[0]);
                            break;
                        case "52": //start sound
                            colour = "#63ae1e";
                            break;
                        case "54": //set colour
                            colour = "#63ae1e";
                            text = "Set Color " + getParamName(a.parameters[0]);
                            break;
                        case "56": //set image
                            colour = "#63ae1e";
                            break;
                        case "57": //set width and height
                            colour = "#63ae1e";
                            text = "Set Width " + getParamName(a.parameters[0]) + " Height " + getParamName(a.parameters[1]);
                            break;
                        case "58": //set z index
                            colour = "#63ae1e";
                            text = "Set Z-Index " + getParamName(a.parameters[0]);
                            break;
                        case "62": //start sound
                            colour = "#63ae1e";
                            break;
                        case "64": //set text to input
                            colour = "#63ae1e";
                            text = "Set Text To Input prompt " + getParamName(a.parameters[0]);
                            break;
                        case "65": //play note
                            colour = "#63ae1e";
                            break;
                        case "66": //set tempo
                            colour = "#63ae1e";
                            break;
                        case "67": //set instrument
                            colour = "#63ae1e";
                            break;
                        case "70": //set background
                            colour = "#63ae1e";
                            text = "Set Background color " + getParamName(a.parameters[0]);
                            break;
                        case "72": //show pop-up
                            colour = "#63ae1e";
                            text = "Show Pop-Up message " + getParamName(a.parameters[0]);
                            break;

                        case "23": //move forward
                            colour = "#d73e1e";
                            text = "Move Forward " + getParamName(a.parameters[0]);

                            break;
                        case "24": //turn degrees
                            colour = "#d73e1e";
                            text = "Turn degrees " + getParamName(a.parameters[0]);
                            break;
                        case "27": //change x
                            colour = "#d73e1e";
                            text = "Change X by " + getParamName(a.parameters[0]);
                            break;
                        case "28": //change y
                            colour = "#d73e1e";
                            text = "Change Y by " + getParamName(a.parameters[0]);
                            break;
                        case "34": //set speed
                            colour = "#d73e1e";
                            text = "Set Speed " + getParamName(a.parameters[0]);
                            break;
                        case "39": //set angle
                            colour = "#d73e1e";
                            text = "Set Angle " + getParamName(a.parameters[0]);
                            break;
                        case "41": //set position
                            colour = "#d73e1e";
                            text = "Set Position X " + getParamName(a.parameters[0]) + " Y " + getParamName(a.parameters[1]);
                            break;
                        case "50": //flip
                            colour = "#d73e1e";
                            break;
                        case "59": //set origin
                            colour = "#d73e1e";
                            text = "Set Origin X " + getParamName(a.parameters[0]) + " Y " + getParamName(a.parameters[1]);
                            break;
                        case "60": //set center
                            colour = "#d73e1e";
                            break;

                        case "26": //draw a trail
                            text = "Draw a Trail color " + getParamName(a.parameters[0]) + " width " + getParamName(a.parameters[1]);
                            colour = "#a6006e";
                            break;
                        case "30": //clear
                            colour = "#a6006e";
                            break;
                        case "31": //set trail width
                            colour = "#a6006e";
                            text = "Set Trail width " + getParamName(a.parameters[0]);
                            break;
                        case "32": //set trail colour
                            colour = "#a6006e";
                            text = "Set Trail color " + getParamName(a.parameters[0]);
                            break;
                        case "71": //set trail cap
                            colour = "#a6006e";
                            break;
                        case "73": //set trail opacity
                            colour = "#a6006e";
                            text = "Set Trail Opacity percent " + getParamName(a.parameters[0]);
                            break;
                            
                        case "35": //wait milliseconds
                            colour = "#3e83be";
                            text = "Wait Milliseconds " + getParamName(a.parameters[0]);
                            break;
                        case "53": //create a clone
                            colour = "#3e83be";
                            break;
                        case "55": //destroy
                            colour = "#3e83be";
                            break;
                        case "61": //wait seconds
                            colour = "#3e83be";
                            text = "Wait Seconds " + getParamName(a.parameters[0]);
                            break;
                        case "68": //open project
                            colour = "#3e83be";
                            text = "Open Project " + getParamName(a.parameters[0]);
                            break;
                        case "69": //comment
                            colour = "#d1d1d1";
                            text = getParamName(a.parameters[0]);
                            break;
                        case "120": //repeat times
                            colour = "#3e83be";
                            text = "Repeat Times " + getParamName(a.parameters[0]);
                            break;
                        case "121": //repeat forever
                            colour = "#3e83be";
                            break;
                        case "122": //check once if
                            text = "Check Once If " + getParamName(a.parameters[0]);
                            colour = "#3e83be";
                            break;
                        case "124": //check if else
                            colour = "#3e83be";
                            break;
                        case "125": //change scene
                            colour = "#3e83be";
                            break;
                        case "126": //broadcast message
                            colour = "#3e83be";
                            text = "Broadcast Message " + getParamName(a.parameters[0]);
                            break;
                        case "127": //request seeds
                            colour = "#3e83be";
                            break;

                        case "44": //increase variable
                            colour = "#e7b601";
                            text = "Increase " + getParamName(a.parameters[0]) + " By " + getParamName(a.parameters[1]);
                            break;
                        case "45": //set variable
                            colour = "#e7b601";
                            text = "Set " + getParamName(a.parameters[0]) + " To " + getParamName(a.parameters[1]);
                            break;
                        case "63": //save input
                            colour = "#e7b601";
                            text = "Save Input " + getParamName(a.parameters[0]) + " prompt " + getParamName(a.parameters[1]);
                            break;

                        default:
                            break;
                    }
                    block blockElement = new block() { text = text, colour = colour };
                    if (Convert.ToString(a.hasChild) == "TRUE")
                    {
                        addblock(blockElement, a);
                    } else
                    {
                        blockElement.a = @"<div style=""margin-bottom: 25px;background-color:#f1f1f1; min-height: 20px; padding: 25px; padding-bottom: 0px;""><div style=""background-color:#d1d1d1; width: calc(100% + 50px); margin-left:-25px; margin-top:-25px; height: 50px;""><div style=""padding: 15px; font-size: 20px; font-family: arial;"">";
                    }
                    ruleElement.blocks.Add(blockElement);

                    //foreach (dynamic d in projectData)
                    //{
                    //    if (Convert.ToString(d.generalType) == "BLOCK")
                    //    {

                    //    }
                    //}
                }
            }
            return ruleElement;
        }

        public string stepIn(block input)
        {
            string htmlOutput = "";
            foreach (var element in input.blocks)
            {
                string b = "";
                b += element.a + element.text + element.c;
                b += stepIn(element);
                b += element.end;
                b = Regex.Replace(b,@"#d1d1d1",element.colour);
                htmlOutput += b;
            }
            return htmlOutput;
        }

        public void process(string input)
        {
            dynamic array = JsonConvert.DeserializeObject("[" + input.Replace("\t", "") + "]");
            foreach (var item in array)
            {
                if (item != null)
                {
                    dynamic parsed = JsonConvert.DeserializeObject(item.ToString());
                    //string onion = Convert.ToString(parsed.generalType);
                    //if (Convert.ToString(parsed.generalType) == "BLOCK")
                    //{
                    //    woohoo += Convert.ToString(parsed.description + " | type: " + parsed.type + " | parent id: " + parsed.parentID + " | my id: " + parsed.myID + "\n");
                    //}
                    //else if (Convert.ToString(parsed.generalType) == "RULE")
                    //{
                    //    woohoo += Convert.ToString(parsed.parameters[0].datum.type + "\n");
                    //}
                    projectData.Add(parsed);
                }
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            projectViewer.NavigateToString(@"<!DOCTYPE html><html><head><style> /* 3D tower loader made by: csozi | Website: www.csozi.hu*/.loader { scale: 3; height: 50px; width: 40px;}.box { position: relative; opacity: 0; left: 10px;}.side-left { position: absolute; background-color: #286cb5; width: 19px; height: 5px; transform: skew(0deg, -25deg); top: 14px; left: 10px;}.side-right { position: absolute; background-color: #2f85e0; width: 19px; height: 5px; transform: skew(0deg, 25deg); top: 14px; left: -9px;}.side-top { position: absolute; background-color: #5fa8f5; width: 20px; height: 20px; rotate: 45deg; transform: skew(-20deg, -20deg);}.box-1 { animation: from-left 4s infinite;}.box-2 { animation: from-right 4s infinite; animation-delay: 1s;}.box-3 { animation: from-left 4s infinite; animation-delay: 2s;}.box-4 { animation: from-right 4s infinite; animation-delay: 3s;}@keyframes from-left { 0% { z-index: 20; opacity: 0; translate: -20px -6px; } 20% { z-index: 10; opacity: 1; translate: 0px 0px; } 40% { z-index: 9; translate: 0px 4px; } 60% { z-index: 8; translate: 0px 8px; } 80% { z-index: 7; opacity: 1; translate: 0px 12px; } 100% { z-index: 5; translate: 0px 30px; opacity: 0; }}@keyframes from-right { 0% { z-index: 20; opacity: 0; translate: 20px -6px; } 20% { z-index: 10; opacity: 1; translate: 0px 0px; } 40% { z-index: 9; translate: 0px 4px; } 60% { z-index: 8; translate: 0px 8px; } 80% { z-index: 7; opacity: 1; translate: 0px 12px; } 100% { z-index: 5; translate: 0px 30px; opacity: 0; }}.centre {position: absolute; top:0; bottom: 0; left: 0; right: 0; margin: auto;}</style></head><body><div class=""loader centre""> <div class=""box box-1""> <div class=""side-left""></div> <div class=""side-right""></div> <div class=""side-top""></div> </div> <div class=""box box-2""> <div class=""side-left""></div> <div class=""side-right""></div> <div class=""side-top""></div> </div> <div class=""box box-3""> <div class=""side-left""></div> <div class=""side-right""></div> <div class=""side-top""></div> </div> <div class=""box box-4""> <div class=""side-left""></div> <div class=""side-right""></div> <div class=""side-top""></div> </div></div></body></html>");
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://c.gethopscotch.com/api/v1/projects/" + uuid.Text);
            var response = await client.SendAsync(request);
            dynamic test = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
            string output = "";
            globalJSON = test;

            // Scenes
            for (int a = 0; a < test.scenes.Count; a++)
            {

                string sceneID = System.Guid.NewGuid().ToString();
                output += "\n{\"generalType\":\"SCENE\",\"myID\":\"" + sceneID + "\",\"name\":\"" + test.scenes[a].name + "\"}";

                // Objects
                for (int b = 0; b < test.scenes[a].objects.Count; b++)
                {

                    // Loop through objects to find correct one
                    for (int c = 0; c < test.objects.Count; c++)
                    {
                        if (test.objects[c].objectID == test.scenes[a].objects[b])
                        {
                            string objectID = System.Guid.NewGuid().ToString();
                            // Add object's name
                            output += "\n\t{\"generalType\":\"OBJECT\",\"myID\":\"" + objectID + "\",\"parentID\":\"" + sceneID + "\",\"name\":\"" + test.objects[c].name + "\"}";
                            // Loop through rules (purple blocks) - for each rule in object
                            for (int d = 0; d < test.objects[c].rules.Count; d++)
                            {
                                // cant use variable e so skip it: d -> f
                                // Loop through *standard* rules (purple blocks) to find correct one
                                for (int f = 0; f < test.rules.Count; f++)
                                {
                                    if (test.rules[f].id == test.objects[c].rules[d])
                                    {
                                        // Add rules's block type
                                        string ruleID = System.Guid.NewGuid().ToString();
                                        string temp = "\n\t\t{\"generalType\":\"RULE\",\"parentID\":\"" + objectID + "\",\"myID\":\"" + ruleID + "\",\"type\":\"" + test.rules[f].ruleBlockType + "\",\"parameters\":" + test.rules[f].parameters.ToString() + "}";
                                        output += temp.Replace("\r\n", "");

                                        // Now for the big one - abilities
                                        var ability = test.rules[f].abilityID;
                                        output += await processAbility(test, ability, ruleID);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            string _sceneID = System.Guid.NewGuid().ToString();
            output += "\n{\"generalType\":\"SCENE\",\"myID\":\"" + _sceneID + "\",\"name\":\"" + "DUMMY SO THAT LAST BLOCK IS DISPLAYED FOR SOME REASON" + "\"}";
            //MessageBox.Show(output);

            // split output

            var array = output.Split('\n');
            var start = 0;
            var previousTabs = 0;
            int ind = 0;
            int increasecount = 0;
            foreach (var item in array)
            {
                int tabs = item.ToString().Count(ch => ch == '\t');
                if (tabs != previousTabs)
                {
                    increasecount++;
                }
                if (increasecount == 1)
                {
                    string concat = "";
                    for (int i1 = 0; i1 < ind - start; i1++)
                    {
                        concat += array[start + i1];
                    }
                    process(concat);
                    start = ind;
                    previousTabs = tabs;
                    increasecount = 0;
                }
                ind++;
            }

            display(null,null);
        }

        async Task<dynamic> processAbility(dynamic test, dynamic ability, string parentID = "", int depth = 0)
        {
            dynamic _output = null;
            bool returnEmpty = true;
            for (int g = 0; g < test.abilities.Count; g++)
            {
                if (test.abilities[g].abilityID == ability)
                {
                    // list every block
                    for (int h = 0; h < test.abilities[g].blocks.Count; h++)
                    {
                        string myID = System.Guid.NewGuid().ToString();

                        // is it a block which contains other blocks (e.g. check once if, repeat forever, .etc)
                        var blocktype = test.abilities[g].blocks[h].type;
                        if (blocktype == 26 || blocktype == 120 || blocktype == 121 || blocktype == 122 || blocktype == 123 || blocktype == 124)
                        {
                            string depthtabs = "";
                            for (int j = 0; j < depth; j++)
                            {
                                depthtabs += "\t";
                            }
                            var controlscript = test.abilities[g].blocks[h].controlScript;
                            var temp = "\n\t\t\t" + depthtabs + "{\"hasChild\":\"TRUE\",\"myID\":\"" + myID + "\",\"parentID\":\"" + parentID + "\",\"generalType\":\"BLOCK\",\"description\":\"" + test.abilities[g].blocks[h].description + "\",\"parameters\":" + test.abilities[g].blocks[h].parameters + ",\"type\":" + test.abilities[g].blocks[h].type + "}";
                            _output += temp.Replace("\r\n", "");
                            var poof = await processAbility(test, controlscript.abilityID, myID, depth + 1);
                            _output += await processAbility(test, controlscript.abilityID, myID, depth + 1);
                        }
                        else
                        {
                            string depthtabs = "";
                            for (int j = 0; j < depth; j++)
                            {
                                depthtabs += "\t";
                            }
                            var onion = test.abilities[g].blocks[h].description;
                            if (test.abilities[g].blocks[h].ContainsKey("parameters"))
                            {
                                var temp = "\n\t\t\t" + depthtabs + ("{\"hasChild\":\"FALSE\",\"myID\":\"" + myID + "\",\"parentID\":\"" + parentID + "\",\"generalType\":\"BLOCK\",\"description\":\"" + test.abilities[g].blocks[h].description + "\",\"parameters\":" + test.abilities[g].blocks[h].parameters.ToString() + ",\"type\":" + test.abilities[g].blocks[h].type + "},");
                                _output += temp.Replace("\r\n", "");
                            }
                            else
                            {
                                var temp = "\n\t\t\t" + depthtabs + ("{\"hasChild\":\"FALSE\",\"myID\":\"" + myID + "\",\"parentID\":\"" + parentID + "\",\"generalType\":\"BLOCK\",\"description\":\"" + test.abilities[g].blocks[h].description + "\",\"type\":" + test.abilities[g].blocks[h].type + "},");
                                _output += temp.Replace("\r\n", "");
                            }
                        }
                    }
                }
            }
            return _output;
        }

        private async void projectViewerHomePage()
        {
           await projectViewer.EnsureCoreWebView2Async();
           projectViewer.NavigateToString(@"<!DOCTYPE html><html><head><style>.center { position:absolute; top:0; left:0; right:0; bottom:0; margin:auto;}</style></head><body><img src=""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAeAAAAHgCAYAAAB91L6VAAAACXBIWXMAAAsSAAALEgHS3X78AAAgAElEQVR42uy9B4BcV33v/z3n9ukzO1u1WkkryXKTu3HDBVOMCSWAAQcCJBAI5MV5EB4lIaEklCQkeY/A40EIkAYhDi1AEppNMeBu2bIs2+pltdo2O/328v+dMytHNmCc/LEtW+crj3fnzt1779w7cz/ne87v/H6AkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSktKxJqZOgdJ/R9/613ufbVvWS7ZsucUCj5OJtcP3VIYLX3zm5RftVmdHSUlJSQFY6Resz3/q20PnP3Xzu2ZmD79my9athbu23oa9e3fglJM34pKLL9pZH5143WWXXfp9daaUlJSUFICVfkE679wXFz/+qXf+dX3Eurrd7aAfBsgVLbAkReZG6Dd7GBpZe9PBg+4vX/70p8yrM6akpKT0s6WrU6D0SPR/PvHpyVNPX/VXUW7uhXs7h5ByF7wAhCyFZnDYZQelSgGaNnO+Xl56A5j+J8jiQJ05JSUlJeWAlf6b+sxnvnTW9EmVjzm13nnL/g5EvAnmRLBy1ILjGXTdhG1WEIUMFrNgZxXfC/U748ze1ZztXLd4oHnDq170JjU2rKSkpKQArPRI9YH3f+S5a6dr/7tYTzYgfxiJNY/U7KEX9eCGHjTOkbfL9LOAcmkIBceCQf9sWmZatDyykbj6XOhp1x8+1PzEsy942Q/UWVVSUlJSAFb6GfrRN7efw/LhbzXbe14yVDMK3aiJbrYA35xHbDYRJB0wDmQZOWBmgmcWGBMjGvQ8AzTdQL5Uo2UWHJ4nJJfAWaG5uNB81x++858+dve3fpCqs6ykpHQ8S1OnQOlo/eZLXmH+89//y9uD2PvYV77+jxdu2/pt8+kXnIlNJ5yCVt9HP/YRwKOmWwJG8BXANQ0DgqZu4ILrHFka0vMQ/aiPpdY8Ds3uxqH5vVhszDrI3AvXTZnXf+crdx1SZ1tJSel4lgrCUnqQ3vSW33/DyFjtfctRi83O7UVRi2HBRuJzBH0uHW0cxfAIroyn5IBDeIZPyzkMwwbXCMVcfLJ09Fp9tOfbWNw3h4M7DqOz0MaZZ2ysmGbpfFrjFnW2lZSUFICVlEj/8InP12u12huiLGbTJ63Gr1z9Mtz9o5tRrW9AL+VINBOG7cBOCMjMJO8bQze0gevNGDGXPk5JglTjWFxuEXiXcWjrfhzechDxfIIivWxUl9EM5jaos62kpKQArKS0ok0bp4c5cydirwmWJTj7tE04YWo9Qr2E+U4Di7028bWFTAtgOxYCgm2UBOB+SiAmKCcaXNfDcqeH5fkOZm47gO72JRQbGUxPQ7kKVAwLc0strs62kpKSArCS0ooywwjnmo0gTZfIqnrQTRO5fAXL2X7cs/s2dIPDsOweud8eYreLlABsOToYN+h3Dr/nIQh8tPYvY89NOxHs6GDYA4qBjjhOMTmax4mnrkYnl+aB/SIAMFNnXUlJSQFY6bjXzgPdg47ZvT0I56/U7C6KRY5S2YYLH/v234iR1SX0uktI7QC6xZClKXpuAMPOI+zFyNwYM7sP4NDWGRi7u1jl6agFGYwkg2+kmJo0se7kMgpnXHBV6dSNW976uu98REFYSUnpeJWKglZ6QF/60meSM05/+szOPbufe3BuX84NGuj25rBj5+3gRh/M8eAlbZiOiTCJqfWmQSP3m2Q63IaLpR2HcOi23cDeHlb3OUbJ/ZbJ+eoZQZg879hpOfiVELm1FbM6Of6MC5+xsWgWwlvu27KsMmYpKSkpACsd37rkotcuLrb2PvvA4R1rDh7ehkOH74NdiFEetWFVGfSijgQpeMrAYw6vH2L+8AKW98yiue0AtAN9DLsMQwK+UQo7BRhj0IrkmOsR0mETSVmDXrK0icmxi9auX3Xh1KbK7Td+Z++COvtKSkoKwErHrV549UvHptYPvUl3upXMWMLqDQWMTpXBTYHdGJ7vww8j+uAwuG6CjhcjbPfQ2rYPfH8P9R6D4eqIvQF4bQ4UNAbLAMyCg6GJMVSHh6FrmpzG5Dj6mrH68NOfdvmZO77+pTtUukolJSUFYKXjU897wS9vZkbzDeXR0JjaWEJpxAaziKIaJ/AG0Ax6DgOZzH5loHe4idb2GWBnE/WuBt7LsNTMMNdN4acZCo6GioBwosFfiNHa3Ueb1rUaKeqwMFbIoV61h6rl4rOefuWZe77w+VvuVVdBSUlJAVjpuNNVL37xRZnevDrkiyzifaTkUsWnJEpFX7IORtBNAobmcgutmTksbt2NeEcD1RZH0dfRaWdoEnxDcr8mud4KwXvYZKiTFa6yDGU3QGHJQ7htHu1bdyOdaWFtrYJKtVAwnPwl51y68eavfnnLAXUllJSUFICVjiu98lXP/eVEa1weYBkRXKRZhH6/h4zcbJQAXi/G8mIXaT9CdHgZ/u4GjAaD3mNIfaDnp7AZsLbOsHnSxsnDDtZVSlhVq6FoGbCSGGWkGDV0VCOgs3sJc/ceRMnWURutFZ1SbsOas4f/9fr/uN9XV0NJSenJLDUNSelBagc7plKzS00zcr+ZB99zQb8AGUO75yLox2gvtMHb9Nrew8jmE4QtAq9LEI4yFOgTtWncxAnDGkatBJUsQQ4cRmLBcarIQge97iw4C+CQO56wbMwdbuO+r/8IZ1QKqK+bvPjcjaPPpEO5Vl0NJSUlBWCl40a9pFHW4MON++i6XQngDAkiP0CYAkGPnG9jGfpSH1qDljc5+t1MTjWq5RlOHNaxrqhhiJMTDjUYPA+N6TJtZRi7CENfTvxNRRoOLYOBEKN5HfNND8lsC5Wp1Ww4X7hEAVhJSUkBWOm4ku8FzNITcqga2u0ePLcn8zwHbkTwjMA8H06PINpwkbQYIoLvCDnZjcMc62scIzpHIdWQ5wUYzCLva8lpS0nko+cR0EXFJJ6AdgCuM1gJg0NEdkUXd6sPK4gRJOE4BiUdVMlCJSUlBWCl40NB4HuZnoLnNWKkjl6XnGmSQUt0RK6LYhjCXPYRzRnwDkcomhwnrsrjhDI5YFGaMDVkdzOLDZl+0nOXkMQhkEXQWYySJmKoDWgxCLwZGCG2048RkQu2KiUs0T5mO92egq+SkpICsNJxpSiK52IvQBgE0vmG5EhDP4Qh+ozDNgw/gdNOkPMI0PR6jpazho/2soiUJlfLQlhxDxbB1dEzFLRMzgXWxUwmTUcmurHDREZVN00dWj2Pwmnj2HD6NKL1Y9gVdLG7s7xHXQklJSUFYKXjSuRJZ2MCJExgpF6HYTDs27UHWeDDIfeadGKQQUa1FKNmAHaqo5Ck0AjUXMwQpk+UpTMJX5Pgm9CjR68kBGtYGrRyAVa9gtLkEPS1E9DXjWAu7eLG+Vns23YbQicfLLa8H6kroaSkpACsdHw54JDtjFMWdRpto9vtIlcysG7dagT9DqwgBHgHtREDw1qerCyBtZ8gT863zA0UdQ0so1VE1iuLI6V/pbEhMAKuVsqD2xpSR0eXpdgVetg+P4ND2/ZittnBwtwi0pSgXqn9a+iXfqiuhJKSkgKw0nGlXte7J5+3D3uddCqOImhZgupQHpMTo7DJDZubE9TtIgxmQNMMMcEIjBywxQi+UQLhnn0CqU+GNyYY7/N9LPSbaCzNoLHcQtcL0A0idGl5u9dBGGTQIoaCoaFSK37b8923fvXTs2oOsJKSkgKw0vGlxbnGofKmqe2W6Uzt23c/QbSH8YlhgmMJYxMjqFRKSDjBNXGRxCmIvej3PLSaLfkICdqe58PteIj7MfpuRD6YyUQegRfC9xPaC5P/8SyDaQLVgtaoFnKfqxVKH/jr/7d/Tl0FJSUlBWCl407X/s2N8bs/vP7Hlm0/G9xEp5mg1ToIxzKRyx9EqZJHzBI5JUknB5zJwCtNwrjbdREJAPshIoIvD7h0uCKJBwi2aZbJ6r+cZ7AsYGTUATPZF6u1/Ae+8KnFO4COugBKSkoKwErHr5qN+N8Dj7/Z4LmqZeTQ67XQ7PpYnBVJNFoQHlYDh8YMREFEjxQZLRTpopNEmlsZ7Syf0DMmqZvBIOg6eYZ8npOrLmLzqVPNDPyDf/7eO+9QZ11JSUkBWOm4Vz17zl1W8abrel3/qjgKYPAUvuYhMTJ4boZ2J0a7myIJAwItRxZzmdoqEzHUWiYzaHBNTDsSPwm8JoPpcORLBnIlBtuJMD5VQKFW2bJn//I96owrKSkdj1LFGJR+Qt/9wZfTK37ppZphai/UjIhrZgSmBzDtFKWSLaEaRylSMrjC+crIZ+FyM4Iv4+A6uWOdEXiBXF5DrqDBzOmiRxvMCJGvZVg1VYaTy3/uw394x7fUGVdSUlIOWElpRXlj49d51f1meQTPibQhRHEHvt+C74qkGx5yu5cxf6iHXjNG7AkIc9ntzPQUlm1CNwjCBrXwdJFJOpEJPXSWojAErJosY3R0OEhi7QZ1ppWUlJQDVlI6St/6xufDCy596v19f/kZvW6nmpGzjZKEAMvAeArTYbDzJoE5lBZYdjPnGJyiCcuxkJIj9qMYHoE3ilMCMUe5YmBitY6hIQMFK3/o7rtmP3DvHb2eOttKSkoKwEpKR+mG635w+KTTpofcfuvS5cYcev0Wut02el1yvj0XfhBAJyBzTVhfDsZMpEkGtx/A9SKEkRwVBmdMdkOPrs5h9ZoKhutFTK0eMyYmaru+8aX9W9SZVlJSOh6luqCVfqY+9JGXP2VsavK523a42HPgADnaHlzfRb/jy2Cs0GVIYwN+H3C7CaKQHHKaiaFggrEAciqnHIno52rdgp0LEaYtwCzDqqT5E884+SM/2nnJqmrhzL88efzFfXXGlZSUlANWOq71O+94xtDb3/lL15x04viHudHbGGkuqsNF2d3suiG8fozABbqtDO1GCreX0WuZnBMswAumkSvmMljLdhjqYwZWrckjV4phOR7qow6GRoui69poNuOnbb/n3tNOPmvslh9ft7upzr6SkpJywErHnf7wQy8cm944clW9Zv1GocRP74aH0Ik6yLQeMhZCNwxkiYYoYNLxCgiLLmcx8VfXhOslR5wwOQ9YdEvrVoZyTZPAFfN/OY+QczQMD5fhODpcr4X5uQY6Hf7c2qhVevMHz331//69W/epK6GkpHQ8iKlToPSxf3jdmvKQ9nIw/1d1MzpZFFyIuQfNMpFlGlotF/ffvw/33XsI/TboEUv3KyAskm9wPnC+4vcoSug5UCzqKJSAoWENTlFMCA5QyCc4+ZQ6TjplCprBCNom8vYI0sxBEGgIPPO7d2/Z/9r3/Nb1e9VVUVJSUgBWetLq1b915ugzrzj3ddwIX9P1G+uCtEUuN4FFzlWzNJh2Dn6Y4eDBGbQay2g2Qszs72NpIYRICC3dr8hzlYlUlEx2Q4u5v04uQa0mpiLFyOUYChUdpYqFyfES1q8bBnEXcZrBEYmgYSHjBjw/Q94Zh85rt+/f03nt65//t3epK6SkpKQArPRkk/Hpa1/9sqnp+tucIt/sBx103DY6/R6SNIDl6ORKdbTafcwcmkG7eUiCtF6rI/Qc3HP3Acwc6MPtM5kBKwpT6YINg2N4xCDnm6BSs1EfLhDEgfpYAbUhGzljkLAjhU6O2YBtcui6jTBO5XSlpSUPtepa2NbIlrnZ/q+94QWf3aoulZKSkgKw0pNC+xpfnOy0Zz6U8vZLNTvkfhIgTS20mj6WFjtodZaxY9c+3H/vISwutDE2Apx0koV8KUU+b9EHJg/iNPbt6WJhPkJGrte2DFSqOYyNlWBYPQzVbdg5jRywKHkUEWQNOV2JUE3QzRN8aTsC2DpkPumU3LCootTzEsw1OhgbX0/uePTWPbsXXv7Wl399l7pqSkpKCsBKT3Rpb33vOZ/ZsGn1Kwl3WGg3sNhy0WslmNm7gCgIYZgmmssp5g52USTX+9tvfBqm13MwOwJ0jjjWkSQDuGZpin7XR6/fp+WhXKbrMeLEQxLHMjOWZemwHROmpcvaDAYvIV/IQTMCgncKlpi0WQ1+GMANE+w9OIsw41i96kQCe+2mPTsXX/GOX//mHnXplJSUnnQ3ZHUKjh+95DVTTz/ppLXvLZRss+WGWGj20fdT+Vohr6FWtpBlGZoNH3E/wabpPJ51+XqYuiczWcVimpFGbjhPLrjbxlCtAMsG+m6HAJwgFcTVyeXmLIJnnv4uR0A2wUX534wh9E18//qdWJgL6G8rKBTMQeEGZHLaUpKKMoU5tJbbBGsRMW1Njg4Pb958bu3b1//bPpUxS0lJSQFY6YmnkY1wXv6rl390+oTJEyOWwE9jBOR4kUYIg550sG7Pw9xsEyxiKDgZzjlnDCefOgov8eEjQcAIlIaFvXsO4bZbtmHV5ChS+vuMEVzJ8cZZTADW0e5EKJWGyfna5IozGIYY89Whm3kcnuvgji0HsO/AIgT6ayMV6FZKW48I4LJwoeyOXmw0kLJIuOd1tWptfP/+Xf82uz9O1JVUUlJSAFZ6Quk9f/5rb5ycHn1jJ1zmbuzBiwI0W8sIwy5cr0PuMyaIMlnlyLYsmGaAp5y7DuVhHR7zkRoGrHwJBwmcd225F2kSY+30BDy/K4EpwEn8JJAbuP6bezB30MOG6dXQeQAmUEs2OKEVRPSzbmsICPiH5xawvLyM+vAQLEdERGdIeQKnVEC71yV33iaoB8gXiqdcdOlZmW6kN9xzx0KmrqaSkpICsNITQn/25x88df2GqY/7Sa/c7C6j7fbRavcQ+C4B14eoV1Qs1tDpE4TDFFkaolgCzrtgHbgdwddSgm8Ne/Ys4oc3bIGh58A5R6FoEXz7CEIfcZJKgPbaMbbcuoR7t7VpvQxr19QR0T680EWURLQPF4ZlI1co0r7JLbd87N19GPXaCG2viNn5eTQ7Hqr0fLE5T40Cj7Yas2K5/JSRyerOf//8fap+sJKSkgKw0rGvi899u3Hl80/9C2juRW1vEZkONHsd6TBdegjYrlq1SgZWNRbaYJkGndGy1Q5OPW01EnKkJsH34IE2rr9uC3w3RS6Xk93Xtm2A8ZhAOqiIpGs6bTOGH6SYXwhwYB+527qDUsUgxxsQqD2CsDCwOoqFMnwvhaHlkPg6dt6/H+Njo9CMPL7znbuRkJPWdA1tajBkWUoNg8BwcsUzz7pk3Xe+/7Xdi+rKKikpKQArHdP64w/97gvNovv7nXivEWpdRAjIhS4TQNsE0zZWrx4jSI5icbGLzrILToC0zRgbN9axaqqGhDHs2DmHH/7wbnQ6CZjGkWYxAi8mh6uhUNCQJZ6c3SvYurjYIocrknA4aLY8uIGLsfEiverS30UI/Zhgz6BpIuCLodvqY7Q+gmazgTDwsG79CTg0207u3rqHh+ScGUvoeEwCfALHsWtD1erE2hPqX7v5uwcidXWVlJSeyOLqFDx59Scf+EStUEre7oaLjhu34EZNzC/tQ+A24HWXUK44mFi1GlGoE4whu5ZFjmbHTlAfyoNrGjnZNu7fcRBRCuSKJnIlE6WhMpx8gQDrEjQjxGGPQBkjTBK4YYCUYDm2ysbYpIEFWmdmvicBqpGbzmidDCH6fh+iWKHr95DAw+o1w5hfXMKuXXtx0kkbvpvG7KvdBm2vDWoY+AToZRye20nuev7505vyV6urq6SkpACsdMxqYtp5lRfPnBukh6GZIYJwMF83IkhmSYq1U+uhcxs+uVnfC8mhJjITlmlrKFfL2H9gHlu37kaWaigVc7CsFPmcjrydQz5fRBDHYLoO03LAmYFOy0e3S/sh12uQi141WZElCWcOLBKkGT0SAn0f/V4HYeSSAyY3TI++10WhXES5VsG+vQdh8fzN7cXsNZFvfamzxLE8H2J5yUNzuYml5j4tV4jf8sFPX7laXWElJSUFYKVjTn/8F2+f0vNz16T2HBJ9GV1vCb1uB35f1PPtImfmUC8PI/ISREFMcAzAyaFqOjC9YRq6WcCuvXPQrQotswnYMQp5A2UCsUHOWNM4gihBtx+Tj3UI6gyeG5LL1cG46I7uyzSUpqljYd6V2bM07sjpSKYlEnNY5LbJTZcL6JEbZoaGYqUEBqO/7c77f7T1ejSyeOyNXtf+vt+3gIRgnhYJ8C46vcbJhUr6enWVlZSUFICVjjmVRhZf0YvvnV5y96LpzRMo2whcH7EfodNoY9XIBJIwQ18ArdOC57cRxV1wTaSMdLDtnn3kiHME4pyMFGCc0w/6JUuh6xkqQ0XkSwU0mwm56jyI3/Tw6TUOOScJohawgWpNwDkjCPu07SItKyNfrMLJFQnOFgG4hDhNCeYxdMPC0PBov9Huy2pI11+7Z8G2628qFIYXHJMAnOWRRjYda0CNgvQ33/eZi05TV1pJSUkBWOmY0dv+7HkjpSHzlcyMEWUBPWK4nosgDMkBe0jjDLXaELnJPsExgdfvy4QcGkvJpTLML5FjdhOkGORpFuO2ImdpuVJGoVgAIwCL6UEa19Dt+QRPLp1wFKUyHaVIP1kul2HbFiYmh+EULMzO9RCGGjljA1lmwjAcgqgppx5VajUEMe3fFKULrfKadWOjR97LNz67985O1/uwKAwhoqs9cuxNkcHL6wxXatYb1dVWUlJSAFY6ZrR+w+jzmRGf6IaedLQimlgAOCGn2Wg2MTI2TM5WR68nYMzJGROkg4AcKUex4sDI6eAWJ1hG5DoJqowjlzehGRr5WgauiylCBoqlkhw3DhKCbyq6pXMEUQuW7dAHi8uCC6KLuTpSGCT6CCGrH2WZhViETHOxjwQVArvORXQ1raBFVqVWXH/0+0mSwmfiSN9JBpsaBLqsO+y6PTqO5MXv/eTpm9UVV1JSUgBWetz1ux94RiGIO69ebMwyEdzU6bbQai0TxFJ02z24/QBr164jt0qwyzIxv5accFu61pGRGrncIsFZI1CKggsaGAFcwNoQA7rQQfYXYRBLZ2xZNjlZm2AY0fbow6TnCM8i/WQBccbF5gnKGYbqBRQqJnoBudgwkjmfIV8TJQwJ5gRf27bpuS6PSdfZqUe/p//4u12HI9/4FEsLII+MWHSd9zoikcjwUL3+a+qqKykpKQArPe4qlPkzTZudJyKZUx6h02kijRIgytBeaqFcLKFaqcL3AgJhIufmike5WoCTcyDYKKoWCaBqmkOfEANOvkiuNk+vkaslN+w4Ngo5gi8BNEd/I8ZwYzHuS+uaZo5gnYPl5OlvTDK5KUpFS+aNFok1RPBVIZeXgVw6PUxdl93bogiEyFgpkm4YpnbuO/7i6bmj31e7wT7b77Bdfp/AnvJBg4IaDprOrn7Xx89dr668kpKSArDS46YX/capdrVaeV0UJ0av78HzXaSCpmEKToTskwOeGBmV1Yl6PVGAIYbve3KsNpezEKUipWRAyyPZXR2SSw7odREgxbgm00+GUYBI5JJ2W0joZ7HkyG5mEcGcLxZgO47MCR0R3ONM1E5ICLIcxUJOZsoS+xQpMFkSDgK6NA6DIJwSUBP5iAWEN+aLfOro9/b9L++d8b34XwI/kw48FhHY5O773sJEta69Vl19JSUlBWClx00nb157nq7zS1OCnyiQIByuY9vyMrebbcRhhEI+B8/z0O11Zd3eKO7DcEQ3syiZEIJpKQE0A9dpmcFh2gaBN5PlAWXNX6K3LrqOaX3bIfeqAX7o0/7Etnpwgy7C2JfgFeuLecCiGzpNY9kY0BkDo+NK4kSOTWcEaU77E8FfxPdB4YYsrheK5qaHvj/fCz8fRdmS6D0Xj8CP6L204Vj6K9738bOn1SdASUlJAVjpcVEur7+o57Zz84uzcuy32+mi0+lIt7jc7BIsNRSKJRn5LMZddV0U6o3l3F8xziuCpgR8GQFXgFHM55Vd1EkoU0KKKUYCxFEYrlQ/ymDnHOQKeZTJAZfKOeQLBkxLpJpMkXMMGIZYT5QsTOXfJJlIWkkQ1gnsYlaTgD4TU5BExaRYgpshocPLNj70/QUHi3eHPvt7r8/IBWtkoC2Z/CP0wqn6cPUl6hOgpKSkAKz0mOvsZwxZBMyzxDSgAgFROF/TNGW3r4h2brddmVBDJ+q5fY9cJ1E3JdTFGUzNBM90+jBYtEwTDJSZstIohsV1WDq9lmUyGQdjg4+NGIf1vEC66YjWE93CkaikRH+rkS02aJsiKYcuHxosg45FN6kRIAo46DKFpeyhJirH5I5j0VUOEQmd0e8xOWx98qHv8Qc/2pb1O8ZHA9fc5buctkEwpz/r9VvQjPRX/vRTlw2rT4KSkpICsNJjqosvvqCcpbkRPxDTfxxyvYDb8dHvBFhc6KDbzQiEpRZnxSz0iKKZQ/AVoLRhmfkBeFMTBnOgwYTJLOj0PPFBy0zYBG/LsGEbefk6p0feKdHzHG3XlFWUuJjfy+g5y9NzG0moI6UHQo6oFyOjn+LBYkZgN4FIjP0SkCEKM+gEZEM+5NxkL6j8tPf57Wvv3Rt7hXcmfqHH0pyc09zvt+G6nVMyRBerT4KSktITRbo6BU8OGRj23W7cy8hpht02UlHyL1fDYruLftdseX3rA/Xyxh0GVv0ti8NKhg78qIeAYCe6gmMx9soIfORMhckVzjkdzPpFLMZtOeTUJNF1rBE8ReyyTk43zVJyxQxi3FkEdwlXnYneZHpNLM9ouanlZOUjcUyimpKY+6uJmUi0THSPyyfC/SZiA2I+sUmNglz1pa+9TL/2U9+LH/pev/HZ2/7lilecOcW5+UGdp3piiPHpSK9Wx17ypve+8Gv/591ffjJUShKN4/TIkzOs4voi9LUe4tZtQXc7LfJ+UTs6ychPXmnW33QKys8aTq14jvszndgLx5HbyDUtXmLB7k7mN11NW7zJX/zb66LWLvWNU1JSAFZa0ZXVT3e+E/zeVsPonOUUHXS6swQln1xuDvXKuk/9+As3f+jd73j7+MyBW5v9zqEKURGCd1maQvwzxTxfoiyXc3INGZUsMvCYJjoAACAASURBVF2JpBcxOdZsJUrZoH3ZlkPPMwRBIEsTimQfJoE5iWMZ0czkP4IqATih9WLhmW1HBnCJKGchsVyMKesmbZGJba90QdNP8RpPNL1o6OynvVfaSzZz7yV/ZZ7RPZHr6WvjxCXgN+GsHrl8zQb9JFpl6xPq4plwxP/XabncS9j4c6Yy+8qIZePLiNr9KLh3VHNGR2A8x8q0asDTuJGL7tib9P68w6LFCuy1Pkt3f9I7eKM8NY9Aq0179fla5WnjqXkiXf/yGM8/4/S4cEIl5rLxVWDmmXR16KrRdaVrP87tsxgz0KDPQ8OOcwTg31XfOCUlBWClFV3+Owxvftd77uz1mwTWBjlVH4aVyuk/bsu/X6wzt9BK253Q6/UimLkYmUY/LUNCUURFi/KApqXLaUicYCm6nGUOaF1MQRp8VMQ83Q45bDGuaxI8RdariNaPCL6ccTkWLAZ2hRsWDlgAPSYXHLmuBK4M/tI06YKjOCHDnMk5wMINa8L0iSAtAn0YhMztL//M93vXHT8ICxOnfjhOoxfkC7m6ros5zMlIHHsXPlEA/Jb89KUE3d/UE3Z6KwtzaRo7J7Py6HBEDRU6zwlzxHm5UqcGiZalK7aY6zG3nrJJc64VVZgtug50JdOTK9UvfBYzb7ittdx8uH3+RmHNlSfx0l9PRtpkTQw70Pk2CLKaaGixRAbK6WIeGcE35nQdU4aGHmE/9xstZF9bjNyPq2+bkpICsNJDpKWlPWG4HAcZ05kRysjicqmAtoXz6OVPxvCylNFtnoCoxamMaE4QQjO4zEKlGSLTlYxVllmswiiWNYHFVCJxIzaEWyW3LKYL0WI5jShMxDxeA4MeU9EVnQ7ALLrC6XXf92WCD0MzZIe2mAcssmiJdaIwFnuSy8WdX3RjS/dMT+MkjO/efih7uPebeMUZU9cOVSuFeqnE5XapxXAuvXRMQ2KyXht9amC/6qyo8N7VgeaIBghDXrpPkQllEC8uOuNTOQQgLobMxi16FMSFoQZKNeWoyRaRiDJnPA/zpa/NRje8pDB8b49lB27zW5/8j2hh7wP7zJWG18fZmSfz/KfP98wxFmWImDzzMsBO9j9kDAP0QvaG6AT9yNBwu+4vfjbd96z9vehO9S1TUlIAVvop4pm5R2dO24/0oSAU3cN9oCi6f7XNJ5+3ziH4xpqRZbZtSmh2/Bgai+n1vEy8EQSRzNnM6MZsEoxFtioRkUz0leUHESXS0QrUDhJmDByu6JoWCTXEeK8gQhCED/zuONZKHuhIrm+ZNuIolqBmg5BquQ2JHOHAxP7o5k/77Wy7ff5hu1TjcMhbtbrezBcSAkhPukbG2ZrLfnm99b2v7A6OxWv0ttz076zvO+/IgY1PRRq0RAB35f1LPbjXPTvqFdkqWlGaPbhtkqN3ewpyZ2URzko0hqKlXzXHupc9zZrMb0hzHya8nxnycHjKtzRqgUm0Y5ARVML3J/eWoaVnOMy97QtJ+L/2+wq+SkoKwEo/U+RnlvrdzmLb7Qzlyiv5lfUUTt7eNLamcOrqtedtu6exNc4ykcDCJ4esy/m6IrgqEN2e0r1acg5wNjBjcmxWJOIQzz0/kmAW8BSO1zAsRNEgiGswMTgddCUTjAfAzaRTjmSAlkGwN2SaSwlsmX8ykVOSxDg0ZDc0gZlcs+ii7vf7sz/v/d52w0x4+gXFZt/NULLEIchu8aF1G2qF7+HYBHDOsCaLkTmSp6bFAgFuSNNhRqlkqzzn/+2Ln0l4M5FLWzheGBvXZtbrHZaddUJiXJkPEpmnW8CWLjU11h5+c5HJcSNr3P3n0d7Lqb20pL5dSkoKwEoPoxTNtuP05sIEJ3Z7HcDOZHlA8jvlnj/zux/9y+d/4LzzztTCbAmWE8lsWSJDlXCvpqERYDP0vUDEZ8k7dCo+HfyIPxN3bUYmOJHd0tK1iru4GN8l5xwhW4EzrRMRTNNMgtjzIgK1hpTROiLPs3B8IrmHYC79PUsjSR4mul5XXDM384jd7NDPf8d3pty8YJGLecx8cKxREjq6nVnH6jV6D7v395/Dxq8tc32MTuP6U1H4nTXMmjYInBadk1KmgSUrMF1p10i+Dgwr9BVQP5SfmYhUX1nI6VxOejpexCf/YH/s8dvpep9lDCNPp1oMOmjpkQYbHuS7RVCeuPQi+OqwnmQ9lv0ZtYcUfJWUFICVfp7+4v1/4L/uzU/f7seNywyTSycbJ75MGWk5ycuWFg6d2urWp2wzI8frEiQjGd3s9V05Fivcrx94sgtYYjsLZTCWcKzC9Qo3LMZZZWBVPOhGFq/nLFvuX3Qti23oIv0V3eRNwyQ+c7mecLXCKovyhaISU0yuT7N1WerwCGg4E+PMDizdRjf1Dz+S96zB6GViH3EiIU7QL41PDBWP2YvUQvzvOHzbkadV3fzsxXrtPNPiG1alxq9eltbPGQnE1K9Bo0RLB9HkHINHRI2MZAWyR3V9PKh7WkKYIL4+sbTV3MHNfBk7aMenJ2W5Gst+GsBTGUQnrtEuzY1uY713fqS3/x/Vt0pJSQFY6REq4f4NpqO9oR+GPHUDOdZbruQwOl5ii0udUxcbc5icGEHf7ROAQ9lFncvZyIkiCmLsNhPdxKFMC2lIOGYyElq4U03TZJey6FrO5XJy+pFISymctJg3LKYyxWJ8kcAubuZJ7MOh7Qa+P1gu5v8aXBZ/EJHWGUFT9EVz0YfNMBgXFlYvyZJupx1Yq2Cc/5T1vFxanf7wRjdd3nFL8tD365jOUs6i42I9OSVK1zLLskUZpyeGmnG49NV47t/gAyfYpc8GKfv1Cd06l7zq+KhuP3VjVkQ3CdCgFQKeYRQOxiIDMR58Ko4eOc5WHHMsGlVBihP1PNo8kMFWJjWMQk12FjzgmI+sb2WDVKEdPQu/FcxtuYgXawftzD3g9kRybzw1N1I8D9VVLR7wb0WH5g4G0bL6xikpKQArrcgLkh/GabaPmDitm6ZMNRknESq1PDlhjrn5JVTLZYKnSLSRyGAq1+vLKkhELwlY0UMppgJFoegeFkFaOel+yU9LHyZccEQ7MExddmHLhBt0B/e9SI4ni/SU2UrXdhD05XziQZe1KNyQyoQcFrlkLju3s5U5wAOwi30vLS9yx+Z/+LWv/t5bquUR8vJ2pMHx+/1fn+103R+1e/5Xr372O+ek09NDXTp2XWw3giiMyMX8qyegdvidpR3ofOjId/M95VO+2QuXL18meI6mNkIi5r6sjyIvwk5+0sUeDWABV1H4QvRsVBIdw6kmhwlEBB17SB/2kR4IsX3Rk5/LeP5CrfLVp5sjy3bKlpfs5Pbl1O+sQunpI7G+LmIpO4fn57Y77c+2EH3nRG3ICFPefG9+211YfGRzkZWUlBSAn3T6p4/eNPPiN57+Zc7stwjnaVoRgrCPXL6EylAeM/u7WG52UMrrcopQHLqy2IKV4/LmrBkWgZLJ+b4icYZhmPT3LhGZy67pNIkeALAwrmJ8VxdAJdgLlysyWxmWQfA1IIK9mIS8mNakSZdsiIIOaSqnIInAahEILeE8iGBGlPhod/rM4Lmzt227FRFBvdXsIgpSGcBlOIVXZdz6lWe9YtUrv/XZQzPcCIvcMmm5yNJliXrFsprDE/HaXVVeM3luWnqFFadnteA3Z70l9zS9jiviGjQR7CbyZouegizFz4mhGrye/WdUcywylLEBbLWf1gUtZmHT+pGWYj7t4hI25EwExiojyVbVmb45hgUjEqEBg1Nb4fa6SW78AX0a/iCXGCKqOvpr97Sb7raW/98tVvCdmzuLi+rbqKT08GLqFDz59PJrzj2N68l1QbxcN/KiZq+FQr6I2dk+br11P2wzh3VTw0hjF2QrB1WRNF+gUgbixHQj5gZkYFbOEdHOIljKoZuvJqcKDebqMlkbWDheUxfTmsihiSpLBFk5j1eULtTYIPmGsNViHFN0fYoMHSKIS45oshX3LABN65tAvT6MNZMbYfECGvNNLM4dIhfvolwro1Kuw8lVwc0KipWx61td99N33PXjKzQjeGV1SJONAZY5C2ToL//9V/77PU+YC2bDeoO2+vILUP/oRMyn2Qo4RY+AndH5i7NHtUVxZO5vxAdBWG0eIw8DZgIZVZ3iJ4Et5w8fBftMG6zXoVZCx2T79rLgL/+gt13Mx47UN1JJSQH4uNHZJ7+Lbbzsa/83ShpvZFYXpaqDfJ5uqUYVt9+2F3v3LmKolsfYSAVer4d+r4v6aBGnP2UzVq9bA003ECYp9u/bj/vu247A7xG0bWQpAVdE0a7cfpOVpBuDO7Qmu7WFzdJ1csSWSGc5qAEssm1FcSA6m+U4sEjoIYKydM2UrjcMPZn+8uRTNuHUU07DfffsxQ3fuxE5+jtxjKeddiKmN24keDvUKKggZTY1EkT/jYG+H+Le+7eg15uFSHqZxsZMs+Ff/sev+/bOY/kabdZyhWcXJq9Zk1hXGFFcrcA4aSKmEyPqJlOjxEyZPM++iEbPfv60of/fNwKWyv1q6QOJROXwQIaHJKU+Al121BiyJHgqnbVI5hETxeetFPex7q0LSL9wXf/gx7Yj6alvppLSg6W6oJ+Eun37H2UbLz3nM6ZhvzxMvHLoCegFBMIYw8NF7N+/iMayD527BMEA5114Kl7wwmdhbHI13Uh1cl5ibJYjPe+p2H/wML7+jW9iz+77oREsGbnaNEoxyHqVwSUHLaKXNfq7VM4B1gbjwL4m3ZPIDy0gK2oMC6ct3C4xBm4SCLwQtDMJ6+mTNqDVcPHFf/kPHNh/EHnHwjnnn43166bEuC6WO0sEYDGv2adj0OVUJm7p0K08Vq+pYX7ORbPRpu3Zruf2vGP6AuWRv5pN/+PpQe4F5SgidtH7YYNSjByySDJCdmQsl5Zk2YMTcjwKkjm30gF4ZQT20Sk55NDxoI7zSpqslSbYIJNWIsE7CKSLicpawjDlM0xolXMPm+xcz0nr270Db1PfTCUl5YCfeLrmQ8D5zyKDFz7yC5uluPTgJzC2dD2BKoRdylCp1OD1OW69aRcW5yOILMJnnjGBq65+OkZHKwS3nASA6Pr0CQxiPLg6PI5GJ8QXv/gV3Lf9LrrDEgDDWFoirg2mGCVRKuf/yvwcMsNShkLRklOZRI+zzLskqinxQdCVKOTARJS0SElpAZdceiF8PyB3fhd8L4aT49h4wiROOnk9DN2E4xQwPDoix6CDKJb9pDG5ZgH2hI44CjN4rofWch/mARdP/fJ+5MOfDaw7Wi573h0HYNKxhdljdhX5S+3xi1dx+2ID7PKz2dDTJl0RqRytOMljuzIol/gVQwaa/DkYh87kkIU4/qPnLA8+gFwuiegk36C1D96Stq+5Pet/bz7ot49e7cg1+L/Pq+MVZxezMD52z4Opp2jOjMD77ClA7di9ddrUGL43XGKvaf4r5iMVE6ccsNJjroxugHuHLsFo5zZ4wSwiN4SR0wluZdRHc1huNJC3GE4/YxMKeROLCwu0fFw61UxOF2IyT/TBmRYsp45NJ2zAzvt2oe8SgAVw40FlIxFAJWrypjInxwCsMvgnJYBrwvUO7sWDKUaQDlk46FRmwRJB1hoWDrewtLgAndnI52JMrRtBuVJEq9UmADvwPDHevCyPp+e3Ua1WaD0HxVyJtpFDN+oi4RGGqgXEsy5Ykh5Tbcu8ntNfY45+7FwUf2MVEUkAzIoH5zmTUcnHfjs4pcZOoBGAEMClz0gdNoYiTZaxpLYXjPTBZ1yWqKQL7kQJLknLqzdZ5a9MZ3M7Phr0N6lv56MnkY5V2SoFYKVH5S64EorDH5lLmDGm0MqPw4oWoacakpCBk+UoFgvkXhsEYwtDQxWZ+1lUO2o1WxAJPExrUI4wTTl6op6w7pHpDOFYFhpuQjdbgm8ABAFkmkphSVc6KKUrEkfni9KFBpc3aBGIJbJn+UEiE2ZkYpyRiSpLQHspwXVzd2J8oiCh7wV92u8YrWPC63K4qXDEnixRmGQ+KtUSRNx1SC680+3B7SzT76JnIKBjMaD1EujxsdW3c6ZhrTud5V897TMWi2xgsqdgAC6ePTE+emxlXnFPS7DEItyfdXGaVcOawJTjw21dZOOOYdPnSGMsbWfeIWrfhdRAq+1Pum0vM3q9jO14In/9jmV3rqQArPRoi/OfSZa8nmLY8FGjR5m75FDmUDN7WDU8joP3LaLdasB3gaKloZDLY3TdyVi3fjOm12xGEC4QNHV02k2CIKRrFbsRc4ijOEWn0QTXcygXTewPyYe6HFk4KMIgo5hXMmKxlX5IkcxBOCYugq+o0dAX84domZPpyGcGKoTQ0UxDlfa6Mwpwv+0hIJerxyKxh472MkFeixEFnhxPdkR6TJPLucpZidAVASI3RLs9K9NdyqIQooZQSq5szofJjhn68l/LrX7uONOuqaW6yVNNjq0OmlEcyJ44H70sYcgnOk6IDZxI72LJyrAtW0ZiF8TY/8x9zPube5LWjRO6bQxpucUfuDO7m0YWjvNC9VvRQht90KfviR0RLbqg++oupKQAfJzpI28VY8BHboWSjhq51qIeYdLqY3O5jfOHGthY7MDiKfzQJ1fYpxa7R46SHG08iT27Q3K4DSyTo20FReza/FvITU2i09wqE2hACxGK+bqhmDfcQ7GUl8DX9EHlpCghx2mvxcLERgS9DEzY3ziSRRWyOACZXEyHXaxd2E6wLMGiG3bBI8iS061yE6NMwxhtp0ROOE/LbD6AZJhm+Bb93VdbHlpGItJo4NBMg9w1uSlTpLKk/ZDjEtOhhkbK8r3Hkue0nVxZpsSUbjIJ0e31sHquc8zA93ecNX96XlZ5y1issxw1LkJZ+3gw2vtw7GVHXeljrhMmG5RLrIcMG40i9qQedqX+zk+GB0UCERdHl8CgS3M33O6T5WuoHLCSArDSAzdni8XIc3KHsYutMwHukkWFOMHShsEtaKmPnFFBLr8Gq88+B7V+H9v2dXCHP4352pm4g2C8dbGFUbNDjrULq6TRzxS2ZUOUM2S6mKsby8QaKW3zvokXYH715qMOYiWlkuyF1vHre2/HNV95PdLSxCPuATYJxM+1SzgjzuEecsN7Mx8tL0TS9qDZCTUmyDjVDOgnmTLIq0PvQTMJ1GEsoeuRrRcuWCT14CEw3j42kmCd4TjrN/DCG9cFJhOJSKJsEKmkZQ8PXhngxGVFZtk9/VMDnI4BieMbizWMo4iTjfLT1hYK//a1YP/LborchSfrd045YCUF4ONR13zopzgRhvkwJx83d8Z+Dq3FHM1Bx2dSMR5Y3OAFXBtuwlXt7RiphuRVYkRMjNqG4AQ5yzZkvmfHzsMgwI7aPnYe3YnIVlI4sAGAxTivxv57sJjkOiblxzE/eH90p0t6YkQ5j11BhK1eKksoappNP8nd+/S7HiNhPqKIoEXuZHw/UO88/lGfFznFE57K65+aSIy8jBKXPRaQBe4TltD51eS8aRF7NZjwlQx+ZzoO6SHuQxtrshzWUaNEJsI4as7tsWOFIY9LTJGqUQPjVMe+bK9ReycB+H8+Wb+GygErKQAr/dclpvywn3LzIIDuLJ+K74azuGzuayjXNTjlIhKd3Kbmy0QaGpODwTAIyJvt+3FjeAZty3jUD5mzQXUkoTqzUDRLyBwdIRfJP3RYORNx4CJLNJmiMuUOJg83oKePL6km6eguY2N/fUFaeWopTAdBVnKcXEzZyWT+L20lUjXQOTxqGJkE4y59E7frbVEFCqelFYymFgF7MM1HuOZjLbn1YI4ye+C4ytQIOlevvt7MGfymcPGDd8S92Sfb10g5YCUFYKVfuG6pXopJbxc2Ne9FELVhlS2ICr8iBSW3NARZAE6UW2/twVh2EIfY9GN7s7d0mPkKYp5A44MyiD23hzik40p1WfzBJFJVm+Hjfi4rudyaPOPnVYNMloYQhScy6XlFnmUdbXoPlggaIyjvMcK53WHrc8zI9Wa53/fi/lVX6avPnfIHSTnClRBpIzsWO6Ef0mCiRtDm1LbXWPZvj+SdizRv5spbw868csBKSgrASg+jVLdwe+kSrGvsh+3EyGIChmHKXNAiR7MY69UIImUzxunWThyKHlsAxzq5XJFLmuvkdiP0PVc6SpEGMwkHqS6tLpDzH3+fuJ7nf6mQMFskQjniFUWpgthg2GK5je3B4ns2asMHQ6T2zWnr9q9Fc7uOxAa/yznhl2qxhpAl5H45uXkmXXPyCOB7JJ9ztvLzF30mMjbIxaXJVC2DYh3Zg/bDECJGJWA4wXDO3KeXf5kA/AnlgJWUFICVfo72FU/EfY0TcFrvTjgOg2XnIEJtYlEbWHSbcgaH3PCZ5IKvc9sIjPJjdmy+bQ5yDxMERNUl3RZlEE30e00Csk9gtmB2Apje4z8BeFy3NkxkNiJyr6KLdsGIEPBkeVlPv39T0vqTf44Wb0H04EJBr8tPXTGd5f9wY2RdaKaDSOl4BbqPZOw3Y0eB98jc4l+wYTbE9DINWNQiNHiIceagJOaBiwxocp+pbCSJzCKlKEMxS59Gf/ZJHHu958oBKx0zUp8opcFNXNOxbehC+iUHr+MR3Prw+i7iyBcxXKIsMCLfxbp8A6doj20+hY4gS5DAFnV/6ebud0M0F9uIgmCQ2jJN4HRi2MnjP/93gQXfbGQBUl3HXZa3/AXMvP6TyaGz/0d3+4v+2Z295aHrv91Z+5uXZeUvnxfZF9VTzkQaUP2/+LXUV7q0BR+MRymrlsjxzBIREqejp6W4PVtC38RKYQ7IfQvwixSVhSDD+Xz4qmtya//Xk80BKykpACs9Ktpf2oi74w3Iugas1EYWipvuwF6Jwgtx7EHXfFxU3AGWPnY5FTqiYlKbnLjLUTZqqNljGM6PIc+L0GIdWqIj189kpPHjrUO93o+bOv/C98zuN76eLbzwc0Hjk9eHzX0/bd1y3hibMJ33rQtMxxDvI+WiqBCOVPsVpz79Ge9Jmly24nvpZ0uP0eTxz5xjzB76+0qA2yM2yysr2QlwWlyWJStnmU+umA+6v0WaUVEJaSVobDo0tLVa4c2nas6EcsBKSj+j8axOgdIRpVzH7cOXY9MM8aLjw66b0Ewuu0TFSGQcJ/ADH9POYYx35zCL1Y/Jcfm6gcMzy1hotOFUK2CGCV3kc+Q2LE2UNrRRCkS8j/u4n8Obo3aDHi/5uSs6KF2djr17LDTraTYonpEdTTsN2Ka1Mc5ymAwthFl2FECzQX1lJkpRmOgxD7cnXQyzAob5IFmGdrQTXqluxGU3soY2nbuDWRcbsyKcCCvX9+fVW+KyL1kEWxlpirONOm5JGljQA1yYDoHHKdKVbbBskJOYnPyIoeur6KPzpIiIVmPASsoBKz26Dq60Dnc4Z6LX9rE8vyxzPotyhmEQy1heMeWnbPh4in3fY3I8cvayqDlMt3OT3G8a5NFvpFiejbB8CJjbm6BxmGEx4EifIKkdL9UKU+9mm772nGzsDVOBIWGVPgSBGQF2OfPh6pns/n2QGZXRVoNaylvMLm4+vwHtV4G9uRbS1IDx0HFwAUQ2yNF9yAjwXcxB45rs6hY1gBP+yIsdyvWI2KORjmdiFF1E2ClzhbMHnPDAVYu52REfhnGGcsBKSgrASo9EjOP2kYuwmBRl9ise56nlXyC3ZdF916JlOqIoxEXFXaimjUf9cGICRGFsGLlyAUvNLtx+hkJhGIXcKIq5NSjlN6DTMHAXLbtuchihdux/pDcaxf9xTlq4pBYkcrrXTwZaMQnkdUYV96cttLRkBW8rEMxEGb8MiyzF3tFlvOJPGd72fhcXvJvhu6OH4T9o7cGXPNIGFY18DLa1KS7ADFM5Zisirh9J7z170HWJYCYp6kYBO7KenNMMdtR+qTV0SpbHebz62yWG3JPFASspKQArPapq5cdwf+UisKSAVtNDtxPCj8QYcCqDsVKywVXewOnGo++CE4KGVnNw/sXn4YrnPAvjq8ZRKdVQzldRsEsYroxhpD4BbhZwz+o1uGf9mgfGN49VeVrc7B8Zr80GRSxE+klRQlJO8uGDog0mE5WGyP1To8JgosAFlw0kUZP3PqOLm6abeOF7OVZPNsH6DTz/pQHcUwJ0Cap8xSnL6UO0vhxfph2JikVVWBKQgvuyWxrpz81NLbbT1wf1nUU0uhjrFRHQZkINNtPd+mO99bkDdrKvY2Ty/ehiBnSSocjt8SKjloRywEpKCsBKj8TqMNxTOQsBinR31pElg5s+59og9XMqxgNjXFjcCTt9dMddI1OHqwW4Y+st2D+7E8WqhXanISOfxU3e77cQeD0Yug6LIHzv1BrM1CvH7KkdYZoznlgXWon48nEJXdFdy2WRhkTm3s7I2e4wwz3/ns2+aH/Wfc9hFvXEvd9KxYhvCtHvcM/mZbzxHxnOv5zOReTKlJBaFKG+Wsd27pEjJccmO4aFa4thEo33GR52ooNTsyq0ZDC/eNCl/PDHLF72LIbb9WUsGZEEez4RtYCBk6iRdkUyXPmrZPfb/iy+9+ztmnttYDAJaTNNESQ+04zMUg5YSUkBWOkRqpEbx0z5FHKZOeQsE4yAF0ehLA8oih/oeoYTqy2cZB14VI8jdgyUhosYGiW47t6CH99+PfbN3o/9MzsQRn3kHA21Sh7VYgE50wHd7nHDcIUApB2T5/V5ubE3nsOHnzcaGtJ5HgGcmErUoRv8jBOjlc+2hkXz1/7enf/y5/zGe/8uO/hPN9oRtYViNAmsd9YX8MzXm5gaaSDxEwKvJuccI+7gVb+dofTrXdyoL8KXFSqosURuW7hYW2TlItdaynRk/4WJwuImYdPfeTxFSI0DkaGrR9dfZDe1ohjrM2vqhfrEM+6KguWb0b3mXiu6dq+Z+F0txRm8VH+WOfFm5YCVlBSAlR6hhDO72T4TXZcwEUQiKBca3XyDoAvXa6Hf7yD0mjgvfx/d/JNH7TgSU4OXjNqxVgAAIABJREFUesiXdExN16HlArC8DzdtYaE5gygR7lfUD06lE+4029hNuNlSto/J81ri+kXVeJBJKlnBr3CvwjXeZQbfuxZzz/inanjxb8/feYN49SPvXXvG6/9k3XM8qy+Dq7awBs56T4LLr+wRfEXwE8GVwJjxRAZzjQ3N49IXplgyB65XTw3ptGNac4/uoTuVYE/Wp4uprXQtP7LjntdCWdu5TNsT8621dKUONB1/wGJ6LZAJwr/gzy5c09169d/FB596i+X9w7yOg2GcHXgyfCeUA1ZSAFZ6zHQwtx47rLPAEofclUEAtmAatqx6xGSBgQSn5WYwpT96FejivAXbtuSUm7GJGibWDIGZIZgVYqE1i9mFGdkgCIMOeBYiDX0YXMeW4TJa9rE1y648VMi7WToVHpmug0Eii8BkmM1nd2Tl4qu/6C1e97l9244UNeYT49p7n3dld5V5bg/XMQ/uST7OfUoIHrWo4WPSexZbIMSKBB4iY0qYYMMaD8NPA+7XWojoG96kx3Wjs5h8a4D3f6GGmSuXcSfvyuv4QA7LhxGjxtgSNYJKzIZD7llkvBKOffCTyznBQRaXjm6/fSNu3f7u7o5XfSI9fMbfewc/pBywkpICsNJ/0QXfVLgAcTwK+DlyvBniJIXBdOjcREY3pCK5nwucXY/eMeg6DNuGZTny+dBQGbajoee3iTUuFpdm0em0kSNIO5aJfC6HSrmI4uo12Dm9GtkxFI91qZu/+ixWPacirK/IuMEZDtgpvsEb1/0bFp//ztlbH+QUN28oPCPoB8+pGAt43fsDnPWRZbz0L3UMV1yC74qDzQREj+qBIBg6Vgev/SMDd65rIyCfPRNlGLkqw8t+w8PEyD6sPd1GKHNVD5oB7CFR0w+Ad+UGIQPCuCYd9pF5vqksMSFGl1NMRCZO0GoX/rT3fGtvbvnJ8n1QDlhJAVjpMdWMPYHt2jQ5Sw0x3ciDIEAcRgjDWDovUa/2wtIh1FnnUdm/Hqfoux4SctuWaRFcKzjxxE0Yq9ehy0qJEZYbywj8gI4ppMZCDNN0YBlFHFw1hUbt2AnImtByG6ciarykySAxBjnIJR57n2cz//NzrQOHHrRuKb/ud19X/fjVL3apBeKjNhzhiucAd9zO0A1K2LptHDNzJpgZDWo3yjECyDFft68RMHu45FUc3xpawOFTG7j0CkKl16XzaKJHdjUmdooGlgD2z2qjHCnq4NOJPpT0MZ7Z4OlKog2s5IcWu6ZlBWasF4uezN8F5YCVfuH3N3UKlB5OKd2kP1d/Lj6H5/7nwmDlcXRaIN18VPY/unseox/9yap2V/znEdKDTNZ9x77R6qfh1iaPkBdd+AS+rhajmfqfPlhItsN78Lobp/NPqdSG1v3wFheNZQc80eGxMpodj3hr4Bs3hLjqpTnMzYRoEwXThCNn67BzPvywiu98U8PVV3OcdnEXQzkN1YoHgd3m4Trm3eDNC6axfTTz/346NVYdmZJ0NHgHHpfJ7ueDmoc+LVyb5UVRReK8Js/6kaQb4qcXBTa1A0QtiejJ+l1QmbCUFICVlJ6g2qX7dxyOwmBdlrNmzQTXofH1TyWz78DiT4Yk33eg+6Mf3nXmgSuf+/6pT//Dp1EtlTFc7OFXX3SdLEox17Tw7btehunxE4h4BRyaX8Kfvv99uPi8p+D8C07Effu/Sc60j42TbTltDBFBVi9gy/b8TX/00fv/Rhjbem56/5RWXWU8JCG0fMoHPx36xeE63CztREhLjpi8JCs96Q/8icj9XNdy4xdkpdEfRZ09ygErKSkAKykdU9oTuQsdjS+6ljG5Twt63/OX/z/23gNAsqs+8/3OjZWrOqfp6clZo5xGCBBZYB6YYLxrE9Z418ZrY/utjXFOa+/aXpY1xuHtGiewsQ1GZAUEynkkTdDk6ZnpmZ7OXV256sbzzrk9w0pCESQYjb6fdKe6qm/dun1vV//qO/ec//l1L0DjqdadLbcnwzD/Z/li15986lP/G4/cdzdmj/wqLlzbSqamet8Pj2DPqS5c+7r/jNSZDt9bL3wFVg5vxK4HbkSPeytSjgfpm0maFcqSU4sFfOmWyie1fHVpsz4z4+riKssqNZ6YgOXyra+e55vi+Omg87Zy2nynE4vfsGOYZrjc9nC2+do0LceOzdT5fP6YgAkFTMhLlNl2u7w/XfsZ35ZvOxbXv7A/6Ox5pvX/9K8+8TXXvvO/l0pD5unTh/GHv1yF6EQILAOjI1XMN/4B//SpnRjoux653iElzQj37f1rdKVuxJtet4g4mcv57Fy9Jeze5+z983+c+mryYCy9hhE/qDx7qZWU/3hiAj47y9JJN4h3yuoffbFT3quX386tX1gZOh9bH6ecpPi27g2vVi1HrbnHwsrs+Xz+mIAJBUzIS5jPtU9/BW185TmuPm4h/Hhj4dAv/Of3h1bBrisLxElfq9gXCKpt9Of2wAn3QMyHcDNNXLtVYLDbVy7ufDvF6ics1DK4/d7mp9Qzq2c3PtEp37tBDHzIMaykMtbjiZV8U2qZlc1T/6t54h/PPv67jSN/8fHMlg9awrwoFGEyyYMtTAjbnl9oyyoTMCHPHX6kI+QcDl2f+vSRW664LIg3j80BQZjMkoQ4Rl9PDa+9ronVAx68ZohplT1nJjOoN1yEIko6Romzg3zNNPYedBY+/U8LNzx+44NOabAQ617ZT3xR/SxH2bviCPiO/aC6+/hm8hip9LdmUgKmrk1tSJQtiRNR+1HgCUGaCZgQJmBCXrKfjwu//fMD/+P6VzQdeCrV6opXZ2QZxxKubGHrhia2bnTUfbE8Y28UIw6ixKLJBIQiRquVwf33R1+fa0dPGGesC2WJbw82+r/Xc9WmULEFDlr+HXf55d988l59OZ79b8NS9r7eLr1vXexgyvDxgD931/l+NpiACQVMyMuEn3hb/4+/6TXWdsdcWs6WMhnomwxhEslIXBVzk0E/nSc0ZYlkjl9TuVdJ2bJx4ng+uvGWxX958vYn0uHx1b6HdZG9fN1XTyeott2xgUes5uf+a/3IB9Tmv2O2jdsqkwtdpvizban8+zbGJiwrljnhHD9/ByAxARMKmJCXEabz2lfZPz42pOTr6fn9lFAdPd5XR9QgEeUzJGeVlg2EsY2Z0yV86UZ3+t5D9fuevFbL80aFkYV5JvtqxxumjaoZzFcN/NpTyfcsjUjuOhW3f+F2x3jNkohueqBVvo8JmBAKmJCXPFduSG9fu9q8zEg3lH+zOH48j8lTES6+sIXunG4yfvqyiHoKQt/P4vbbchAqzY6Mef0ffMfQf/nUF6Z/G8C361aO+M6mkchNhK57MkfC0GOV24+ZrV/8eP3IM9YXVXIO/6I98afqyz99uZwTJmDyQsPfKELOQVaNZlamMhl7596B+Lf+2N75ua9Kf/VaiVxaX9995r5OsVKwbbSw4xoPoUptg92m88Pv7PrlV16d2fT49SIRNjwsX1vWMxeWHYnbMf+nH28c+UeegadOwIRQwISc50zMWHf+xd/Vfu1H31d/78H95r1vf0vaXLt6EY7p49mm8tUJ2DYlHGliqD9CqdCC8EQ8NpJ5gkGW0PY6Sua+oZKvHeJBq/7V3XH9Yzz6TMDk+wOboAk5t7DXdOd2DPR713bnzbHf+BXnx66+QmzdMDyDuKXMa4YwImO5APMTrbtMUhvDxPRsEbEd4JLNEi3Pat71qPc7n/78wuHHP6Uo0lszZgbHRCf8Unj6l77SKf+l9gxPwdMnYF4DJhQwIecfxrtf1f3O665zf37rFrFj9WhbDBQDOG6kpNpB7OsxtzFEbCWSTeoxQy5fCjYNdNoZBHGEfK6TjC+KTBP/+qV0bFvxX6/dZP3lW37s6K7vTMrmZ49ZQXAgrH/mK0H533gKmIAJBUzIy47utBh6y9tWfOKaHUuDKaMGR4m13gIyMOFaDoyUngfQwplJgJOpICFttIIUFsoCt91ZRKvewfv/vYuUaGLFYAtbN5YW3vT+I3oc79xTveafN47pa7283ssETChgQl6+hDGmb7t7+k3/9qXWhepPfaaUT7uZvMwjDD+wYU1mbSol4aZspDLJiCTEgUQU+srFStCuiQsu6MA1BZqNCKlirJ7mYHLSP4BkrkbCBEwoYELIU1LzZPz3X5jfrb7cvTzR8lLy+Ifee+GDr3q98/mcM52PPB+RH0EYArYrkc8FKOVDZHTP6EAksyRFuvBzJNBWy/R0Szc7hzy6TMCEAiaEPE/+8tO7b7Gt7jeM9Vmf+Nn/IC93nPKZIcBnrv/qvlid5bKTuvezLkcpVFCrVhwcPhGe5BFkAibnLvyNIuQc5xN/W75//9H0X883UyrPiuXlTCXKs4hv13NWt6ZEpWrh1GQ0zaP3wiZgQihgQl5mPLSvetepWacB00EsxLO+q6s1Q5467s3wyDEBEwqYEPI9sOdQ5eCuXfJLzaAbOv4mg5CESEpI/t9BwGfjsIFKQwST1TMXkgkTMKGACSHfNfJfbpj/w8cOOfOGmYKIdWcrG8sTL8jveFs321HgIWCfISZgQgETQr5Xbt9Z3/+VG9sfOjY32JEpPSY4UCJ2IOWTErA0oey7PDkwYQImFDAh5HvnD/6/2X/7t6+Lnz5wck07dlJYHmX05GvCAmFkqPe2Y/KIMQETCpgQ8gLxkT84/vf/68+qv3p8slePO4J4cgJO7sbqO52IR4sJmFDAhJAXjtTf3VA++cheI4KjrwM/eWKGUA9FSg1tuOBjQ+u3/7R6ZAMPGRMwOfdgIQ5CXkLv19H1F7wvO7bqp01/6UI3f8LW14EhTMCWyyKWuk6lhVCaIt3b9bZ09/Db1l597WJt4ugH99xx85d4CL+3BMxebYQCJuRlyOi69T+VX73uk5mBURjlCtauWe5n1fK68PAeYLZewqmFPCab3ZgONmLrO65Bad12dOVzPYe/+JmPKAF/XW0m4JFkAiYUMCHk6cmu2H5Z38bXvDlX6im6D/3Np1df8p4P/Oqi18LcoUPYMTSD9QMBoqCAT96yEjfcvREDO94BuWULuuAgncogo3wRxCq1mQYKq9asVdvsU8sUDy0TMKGACSFP4oof+altA5s3fyjb33+d1T0wJLID6ZwRmqtfX7GcVRvR15zC9F2fxY/8+zbcbAdf+NYI7jf/E7Z/4BUYv+smbLhkOxoyhUiGcKMIVuijs1TD1Ph+XZSDMyMxARMKmBDynaR617/qlV9Ob7hydSAchJBIqWV+14MY2rQK1VMTGN1wIdasz2LH9gVMTWbxz3suRd9rrodIqTdzNgPv2DjMNdt1OWhE0ycwvn8vSikLpe6+sbf9zsf/oTU//0/f+PM/vFm9WJvHmwmYUMCEvOz5pQ+Njuw8KH5bSLE6Mm0gCGHAhFDv0HJ5FuHpgyhu3gFnxSqY/dsxMz+DL9yTgbn5JxHaBoQnMLz1Cpx87D6s2XgJ/EYVxx6+Gxte9yNw0nlIEabDKH631aq/+32XverBqT0P/OGtH/8tdspiAiYUMCEvT977w/0XveutmZ+77GL5Q/c+Yvd/rhrAFAJCd2gWBvwoxqYrd+DgjXMY2HohmjJAfsUO/N3Nt+OI9VZ0rdiKTlyBodZN9RZgGS5aO+/GqYOPoHfVGMxsDvXITzpI25GBKFUARnNXrO7qveFNpvW3N/2PX/tlsGmaCZj8QOBHOkJ+EH/MYaz4xG+P/Omv/5xz5xuvWfqJ4eJ0/+aVTfiVQ4mA9YxHAgFSZohD99yJ7itfjShdgvRj5FZtwu1zr0fmih9FHLeUXF21voQX2ei55BVouw5i6cHsGUCgxGspiVtRUrFDSV23TRtoZ3vE8Kuv/4nrfuY3/3eqWEzzjDABEwqYkPOeLaOlN/3Vn4zd+f53+h/euGIu78gGYt/EhlEfK/1vwJ+egjBMpB0TM4/eB0ul2OLYBgRRiFhF2TjXj83v+QhEqguh0MWw1CJNJV0BK5NF3/Yr0bvxQvWoDTPWhbH021wsz6AkDeVhE0akvs70YuySHW9MpQsjPCvPLQETQgET8hLl6m1dP/77H+36wnvfubS64FQgo0il0lhJUenSquCnXreI5p3/E6mwjubJo1iaHMfY1dchDAIoxSYalTrE6rkW4rNCWK6ElShWPRb4Pux8L4KlORgq8SZriThJwEK/lnq2aZmIarPYc+OXPl+ZOXWcZ4YJmFDAhJy3jPS6W37uJ/v+7B3XV9KW11DijZbrNqvkKpQQZytd8VITNzQfu+kDO//Px2+Z3Hk31r3u7WgbdpJetaSFPFN2UsqnfZ1ISb0wNIba9ARkqFKzMJPnSi1qYcBwHXSmj8G65w/xwcvuyfHMMAGTHwzshEXI94mrLk1dd/XlrRLCCvR0vlqIhtS5NY1HD3RXbrnD+7mPfuzYZ/S6l72jfcn6V7zuDXGqGyIKznhaPMdXUtvNlZDp6Ud1fBfy23YgCk04KgG3Fsuo7Ps6LsTn8LPvnES2S7wrnx378Ps+PPFxniEmYEIBE3Je0l2wbcdWqdfQbcgiuRYrVAquBwa+9UB84KMfm9fyFcXh4Q8PbNz8U0t+iIJKsyJpYhb4zmkHn86/El4YYPjSa3Hktq/CtaWMpdOoHbs/vz73Tbx9+wSuuNBTu1FLGrWv2jb8u//5XX13//nn5x/iWXrmBMxe0IQCJuQlyOlT/vFyJY/hXjOZsAi6CdqxkZUOxlYWLr3g2pVfX3nlj7jpnp7rrOFBYWRySZOzHo4kIJ/z64gzzdV+uoAN17wOJ7766fHb/+av3vz/vD77s7/437s+PFhcBLwAkWkk+7BmuJa/7triHykBvwHLEwwTJmDyfYC/UYR8n7j5/tpt+w/HB6OwqBSpsqebwqMH+/Gzf7Ue/zj9Qefyn//k9YNvfOdrCpe/UbgjW+Dku5POVsnQoeeBXG6wRiQ7QHEI/asuNEMEp7/wjcpv3XyjPBggpydMgqkkbcTq1qpj2yb56rdcVnwNz9IzJ2BCKGBCXoKovFu7/6HWL52uZOqB3YVPfaEHv3Xru9G45i/Rd+2/Q5zvhR/HCCMlziiE1B2thEharJ8XusczTFhRRiVhC+XarKce0A9WP/Wv1Y8dnywkvaOXVS2T1xocjMVF27vezrPEBEy+f7AJmpDvIx//+/mvbVq35vf3V4f/eHzgp9D31jdABrH64+4n3zeTJufEot/1a0glX13G0qjPQvf2Ks/MVdXDxQ+8O3vFQCH7mmOH2tgwauru0mefgEy6ga7+whWJuZPPCuSpEjCvARMKmJCXLuJjt181cvG7fgS9W66EbOlCGr4uTpVINz7T01l8Ly+QlOtQCnZdHPzmF7E2vOOij3125aMjI+2B7i7P8OsNIIzPvMpyvLaNEOm073yPL80ETAgFTMi5yWXvfP9HL3jXT/+80T+CsN2GNJDoUpy5GvRC2E+PL47VW9tyuzBw4WXYUPgX9xXX1IcivwpTZVvZq2t4GMvlLpV4TdhYqveg2YxuAjthMQGT7xv8SEfI94lN113/ni0/9J7fNfqGEYZhYtvlwUUv7NvQ0OUmESGUDeTHLsDBYAeOTLjJ6+i6WHFSj8OApV684xWx7/jQ3K13ux/5oz859Rs8S0zAhAmYkPOK7Miq1Vvf/O7/ZY5ttTuBfHHfeMlMSjIZZxzoGtGr3oZbd92J9SsbiGQEz0upxGvhyHHr2N6D5r/+/T9XPvXw4ZNHeZaYgAkFTMh5x8Vv/bGP9m6+erAViKT4RpwkVZksuilYPq7xOUnFj+v5rDtlxcno3uWey8vr/N/rt7ohS+qiHmdqRSfr6W/HFoywo+S/Brd8aSjYUPIPeWF0dGFGPvDgw617//zz03vUWhWeHSZgQgETcl6S6uofGdly0duDVA5GFMDQMxgJS2nSVqK0YYowaTJ+OoxY/eGXRjImWE87KJO6ztGyjnV9aITJEgsjUbC2r6W+GRvLWrdSaUy2th553Xtvu5bCZQImFDAhLxs2vOKVF3SvXNHfMU3IRh2d1hL8ZhXCq8NrLkFEHQReG/HZCRbEct1nGSuROikYVgp2KpsssNOJUA0nA9vNwlD3heVCmkaSgCGViCMtajxuwgYDfcOj6cdFZsIETChgQs5/OrNzxaNf/TwWq6fhZGYAc0mpMIAwA+XGCEItpvH4Jujl2s967t44EkmnKe3OOFKJWTrJ3L9RbMG0Cuq5ebjZbjj5PriFYdhdY3AKQ0gXepS807rkdJKEne5CXm1EL1WeESZgQgET8rLg8IN3fr01N/NzmRXhL6+60l0Zmh0s17tI5i1S/5jP8Gx9zVgbWF811s3UnW9PSSjlUhJy4zhGW2m1OW9B+o5Kzio1uwNIldYh1bcNfau2Ks9Hi2DzMxMwoYAJeZlRnzxx+JNr+kZS0s/9iUwFOFPu6jkgz7QkG497RDwuKyuFW+ptbEuYaT2/cAOmbMII5xH5B3H0mzfieGtV0Kl1Pqme0OCpYAIm5w78SEfI98vCc80bvZasC/HCve2EONMbOonCSsRRBmGcQcfMKCnn1Cfs+EinPvfm8V13fZJngAmYUMCEvCyZn6jsa876f2v5LoykLIZYnjhBfI+z7Og0nQxDkmd6RPuw0YbdjiDr8R+OP7r7Vh79FyYBE0IBE/IS5cSDs7/fmJe7TamyqegkEydA2i/AluW3Rwkvt25baNWCncf3zXyBR50JmFDAhLzsWZyrLJzYM/e+5mlj2kIusaV8AUcHJdvy1DbLmXsWTzV+tFVp13jUmYAJBUwIUUztndtz9KHZTy4eU3dCE5Y4U70KZztYiTP/Pv/JGfQ1YelZOHT7/O/tu21qnEebCZhQwISQx9GTL3wxn860qvMBanMqt3oulIphGmEy7Eg3TSda1kOQxPNJyAKGZaLQl0rxKDMBEwqYEPIk9u8aPxjHzXsGBnNwXBv1SoClmQCtBRdBy4aMZPL2TG7l88jBanXDUkterONRZgImFDAh5DuJy/Otf4qCNtxSG4UhgVyPBcOO4bdCNBZiNGYk5sZjhA0X4nk4WFgS6YK1jYeYCZic27AQByE/IGoT7a/3rcyeFiljREVdJV/AdUykktmR9GfjCO6SC6/lwyno6Rf0Y6b695kKeegm6wjZjHOZuqPrP7d5pJmACRMwIeRxnDo8N1dfDL5sBKkzQtX1n2VSWjKOpZ77CG7ORBhEysXW8jhfKZ/1knAsYpiOGOsfKQzyKDMBEwqYEPIULB5vfD5u6+oZ4tsJ1pCxWpIpkSCsKBFz7C3P96snJRTPMmxJC9hwke8bKa3lEWYCJhQwIeQpmNg9c3+nKg8LYUIay3MC66kIdXUskfSCjuGkTXSaeg5hLV4LzzqroG6FdoVwuqzLeISZgAkFTAh5SlfKVmWq/XX4y2LV0o3FmeIcuryzlEhlDbRbusTk2aFJz9wjKxlFbKoknRZX8AgzARMKmBDyNMyOL34+qgnfENZyepVPTLN2KkquC0tPl6xUXz9beQ49MYMRwsnYG9TW0jzCTMCEAiaEPAXlydrDtYX2g4jtpPDGk/UqlUxNx4TX0M3QMZ61PpbQV4sjuBlntHesMMIjzARMKGBCyFMTLh5vfDZumYhNlW+fNMQokgYyBQOtuq6PdWb6wWcMwBKhUrDlIN+/ssiOWEzAhAImhDwdk4dmvthZjOcs6SB+0jijWN1NpYEg8BFHzrMKWF8D1puQTiycnH0Bjy4TMKGACSFPQ+zLqepM60YRLDdDP1mohh3Bsgz4bXVPPPPbVifoJEXbMVI590IeXSZgQgETQp6B2aPlzwQ1EcF48jXeEJG0kc476FQimM8i4GWBq3VkDDsndElKh0eXCZhQwISQp6E8Wb2rPtd+2Iyd70jASTN03kC76QGRiWdvhk6isK6ItbpvbXEFjy4TMKGACSFPj7d4uvmvsmU+YSySIQ3owpTCjZIezmEHePbZGfR4YhWmHRQGVnRv4qFlAiYUMCHkGZg9unBDe8lfFIaZFONY7vMskzKUUgk1k7PRrEYQupn6bFPzU6KVrRKwpbydta/kkWUCJhQwIeQZ6NS8Y7W59k0IrCTkJr2ZxfIEDHp4UaZooN1QApa6GfrsmOAnd9qSyXMSPRsSTtq6nEeWCZhQwISQZ6E8Wf1bWTcicaaZ+ey4YH0d2MxosUZA21bfMJJa0d9RuANniloqSUsz0h2xNlmuWeSRZQImFDAh5BmYPli5q1XxHjZgnkm0xpmkKxCbAdy0iXZVT6Aklh+VT/3W1uKOEMJIyeGhse6VPLJMwIQCJoQ8y9/6xZOtf5VtC5EZQ55pbjaWoy2yRQu1hr+cfHUHLePJyUyn3yi5bhzHhhKwcIvD2W08rEzAhAImhDwL8yeWbvArWDKlnTQzn0WqL92socQaIfYMLHfRemb0sGEzZV7No8oETChgQsizUJ1tHqvPdW4yImv5mu8T3rU+0pkUvLqEYei+zs9SGctS6S1vbeP7nQmYUMCEkOeSgo/XPh01DCnNJ2ZcnXkzRRutZpiMEdbN0E+PTBK0kzI3G645wKPKBEwoYELIs3D68NwdzSX/kG6GfjyxFHDS6lYFMhmazzY5IfTcSHbK6O8eznNmJCZgQgETQp4DrcZ882bDt/CEsb4q8RpGiFTahNcxv7N09JOIZQwjBaN3sHsVDykTMKGACSHP5Y9+I/znsGW0heVAWAaEKWGYArGe6aiYhdeKlIzF8lAlsTxYyUgWeeYL9R1dDsswVAq2+nhEmYDJuYPFQ0DIucuRndMPu81od9dg6iqRsyFsM7nkK+ErOfvw5jvwrR7IfATdUq3TsBGbiGIJoacurIWIyh00Ttfj2ZO1aR5RJmBCARPyckOHUxNnK2ostynrSBU9zfrpS/Kpq9YX0v9lk8DF6cUqmvMxosS+FhwZIWME8IwS7rx5EpmeFEw9ZlgPFlbrOFGIjJJwKYzRIyRsyzCmu9N/ftx23nLT6Ji8AAAgAElEQVSs0fnM3mbnPvXyNZ6W55eAmzwMhAIm5JzCXGM4vcM5e3Uu7azLGdFK0zRWGJB9advIOYZQLpSONAxXyKRshl6UQhGqx722L+rNAC0/jk/XgmBpoe2vXNNTunRbydoylg5FKvISX+eVvpVjlb196OqUy4Gsg2Hl8EssD3YUQIR6TTOpIx2rd7ehl0ibPkK2FHePFMwfv6id+vFXec74XIjb5wLjptM1b+eRZvO02lhw5ucxHrecRX9gCIFnHXbMBEwIBUzIi0r6omzuFetL9vWllLOjxxFjOSMYMPTsQ8p+ei4j82ypyLMh9+ykCmfy73KJZwGpS0aqJVC+82MD7cBCNQxRa4WY6MRYVVCJVgTJ7EaRSrim7oQlTdgq4YbCRSdW34vVfRkiMMNk0oYwqRMdwYwMCGXiZEKHUCJrhMimTWRdc20uMNamO8YH7Tiq9LvpY91pezprmylbiJylnySkZQihPiOol4tlFEF0vFjWwiieDCMxWffkkalmePCg1z6mfpAKEzAhFDAhz4kuy8JsED7v541azisv7M1/7IJB57IB0UQqbikxxolVzdCGcihibdr4TJWqs3P3yjP3z2bIePkL/W39RrTV3ZwhEKckBnVsVVKOlYx13x9D97FKpCqSGNqOBBo+MKXS8Wkvwv5KjEHHRDolkFFJ2dWxXMlXq9+zlz8ABL6J2fayKS0l67wtsFa91pa8KNlG6pLlNvI4WXTxreXJIM7so/pwECcfJZYXHZU7kYVGlA1f7xWmFr3OzmM178v3VztfVt9aerHPARMwoYAJeQrEwkvjt/67/cO/eaD7L0bT5la/3UDTNrXNYOneTzKG52BZmGe09UzttYngzjruzNc6FevKVnqsr0qdCJTIa0rcXmCqxYAX6bSsE66pbi0ZSLkvlp2H5mBerl55m92OYIcxbJWULSW3vBWiKPT+uWiLECVXYpVtwVQfGCIhkjrTRhyp13pi63L07QkgJJY7VEfAmZpbenFVCs+pVN7repZMyZWhECs3duffsaHRc+CBqfIvHGq3b3kux/KlIl8mYEIBk3Me940q/syf2zPf9VhN+eaBTBJLv35j63k/v9Zq/YdKKvd7TZl+Q70TGoYSYloJKaM7QNnqfxUftZBNw0iqViU9k88I+UwQTtKkvtW9lWP1XH0bqPteFCmxBvDVY6HKsVrC2s2RsFSydqJARnOh9A81Q9wzW29986Gl6kNqkw2Um8Vru7KvHyy6b+u27GvU9lfWQphzwXLSNo0ABaHTcYQF01e7acI21aIkaqu0bZwp9iGSPTvbN2y5eTzZZ3F2gJN+TH1AMMKkSTxQKdiLJFrqtq2SuIVwc3cu/RG029/Ac7he/ObrMy+Z320mYEIBk3OT0Ed4iY9TK9SXi1XIkgFRic/J23Qliz9ANpHDH3zku/txtTyPHuxgfI+PmRMh6ksGlnSfYt9XvrOSa71azEl21MJKUm4yS6+6UZJTX8e6udkwky5ZlvKbvmaczaZgpWLki0p0rkCuy0Sh20Chz0DvoGUW+1JDxXxhSG3t1Wr5dWDoO/at2YoxezrE4slALWq/5iLUF2KU6wKLkUq8zUCldF3MQ19L1terTQiV4m3TUPshk8ekStjJfMNy+Tq2/ll0ng90+o7VcYxF8nNFOonrH1v9KTHyJoY3GXjXW/OvHRrpf96DZv1vBkzAhAIm5HtBS+6lcPu9su7qTLIk0msqwc2H8JT8VExFqyYR+TgjXCTC1ddxE9/qwhn6uq+rS0oaSd6EKdV9IKskllKPZTLGty8df8fxfZb9yqifb/WwhdWXp7792FJZJd+ZEOWZCBUt5HKAdkPtZxWYn5bwKkquat+VktUHAVOJWKVZI07GTekErzt+Jdef9XVp9f20+lBQHDDR1W+hNAD0jJoY2eCgt996Tvv4ZPSHIyZgQgETQp432ayhFuec3b+ubjNZsIXnigmYnCvwIx0hhDABEwqYEELO3QRMCAVMCCFMwIQCJoQQJmBCKGBCCGECJhQwIYQwARNCARNCCBMwoYAJIYQJmFDAhBBCmIAJBUwIIUzAhAImhBAmYCZgQgETQggTMKGACSGECZgQCpgQQpiACQVMCCFMwIRQwIQQwgRMKGBCCGECJhQwIYQQJmBCARNCCBMwoYAJIYQwARMKmBBCmIAJBUwIIUzAhFDAhBDCBEwoYEIIYQImhAImhBAmYEIBE0IIEzChgAkhhDABEwqYEEKYgAkFTAghhAmYUMCEEMIETChgQghhAiaEAiaEECZgQgETQggTMCEUMCGEMAETCpgQQpiACaGACSGECZhQwIQQwgRMKGBCCCFMwOTFwuIhIC80osI/VOT50X97Wv0bMAETCpiQ5/+b5KB0NMboXUDTScOOajwm5HkQnPN7qBNwkyeKvIDwIx0hhDABEwqYEELO3QRMCAVMCCFMwIQCJkTyEBAmYEIoYPL9R/AQECZgQihgwgRMCBMwoYAJEzAhTMCEUMCEEMIETChgQghhAiYUMCGEECZgQgETQggTMKGACSGECZgJmFDAhBDCBEwoYEIIYQImhAImhBAmYEIBE0IIEzAhFDAhhDABEwqYEEKYgAkFTAghhAmYUMCEEMIETChgQgghTMCEAiaEECZgQgETQggTMCEUMCGEMAETCpgQQpiACaGACSGECZhQwIQQwgRMKGBCCCFMwIQCJoQQJmBCARNCCGECJhQwIYQwARMKmBBCmIAJoYAJIYQJmFDAhBDCBEwIBUwIIUzAhAImhBAmYEIoYEIIYQImFDAhhDABEwqYEEIIEzB5kbB4CAjTDDmXJKfPzbl621T7GHVJmBA8WYQCJuS7Yf9MmPwF3fFXp3gwfoDc+9Oj3/56bKBXmw2BVHfO1Vv95ZL61enmuSMUMCHPO/lq+WrxDtj89f9B88OfmsZsEGLHz23B+gv7zvn9vcLJ4TqeNvICwYsa5GWL/sNPeA6eK2Un4gkjFDAhLwRMwDwHhFDAhDB98RwQQgETQgghhAImhBBCKGBCCCGEUMCEEEIIBUwIIYQQCpgQQgg5h+EgPEJeBKSU6sNtYAjhhE98LJILR/ZfffrQoV/cvGHd3zkb1j4wV5lPl8vVi4f6VtxWzPe11KruiZ23vt+KYXUPrrmpaWen+4cHG9OTC7nq/lvfPNJfrC8dO/Jj7VrlquKqsZ21vpGbg1Thji1rLj8uhJBP2I/FmYFdd937oZnaQn9uoOtT177x3Q/z7BBCARNyXtLoVByJhjV3eqr/0K4Htg+ODN/T6s1WlRxDKRdF12DXxJHdLbln167/uLWYrc3Pnh6Fmz6u5NtE7KVP7H7w4sWZmVetGOi/pzxzbEP32Lr5sDbedfDWz74lqC29pzwvMrNT068YWbmhUq2FPU2rfvLia157QstXTk0JMTwsteynjt639stf+ZdfQGievODSK39vzcWXz/DsEEIBE3JeUZk9mfbb7Vzf2Iby9MTBFZ2s68d+iJmJ4z9Rnjp98VXX//AnpIwqQphy7viu0XUbVx/Yffc9v7Dvrm852ZHh+0e3XHmD2owZ1hYGq5MT7ynYYsmFt35ubuqqUjHOxNOLw8bpvR9KVatrWlLafT2DmJ1fnEiv7P+vl7zih+9I0m6l5jYaVVvKE62ZqZ2jO++9+zd8L5h413/6tU8oObd5lgg5t+A1YEK+B+bm5oyJk0eKha5MptOojBx7+PbrelLIBXOTFw/krb6iLefLJ8d/ft83bnifertlpTwqQq+RCzu1yzePDbRO73vk+h60M7nq+DZMPnrlwu4H3rNwcO+7hh2MLU0c6pfVySvk6cd+ZfGRO34nPPrIRmvumO16NbRajXZxePCTm9767+6QsiNmJ4/0Hhnft73hl7NRIy4dvu/+H5fN5ugbXv/ar81OPNA9Xtn3hPnzdh48jlPjJ8zJffv4N4AQJmBCXnr4fmC5tpVfnJ4u5XNOYd/BXT+Lxvz+bMruqrSrjZHuzND80UauOnnslxce+dZ87yWv+eLQYDA1cXxfl1yY7ioEFbSO7X5vbHlbpS+M+d27rhk0/LxZnbrMLk+7XVboNA6fXBdPn0CxWUYqV0KUSqOdzc/kRwaPq11w547u23Dq5OmrcqXSrsHV25unHvrK9bWpqQ9u27T1W0vTM5cH2czs+lVXPeHa8PpSypk7sXfYsdxpddfjmSSEAibkJUWpZIpqNcwZiFO+13xj5NX7juwe//A1l10yW52bdEZWjz3W68TNhekTI63B7t+cfuiL9lBfsZNqV7sXJg643f4iOkfrw02/MmzETtwe32PkcxnEOb+vcvggMikLlvRgLM1BdDoQGYFWbCLXN/y1viuuncH8vitnH77/503Hnlixsf9u1HevP71n53/sNTCQN+PRmcriN7bveNu0lNI5s8sG6qcz5aOPXjpzdP/aK9/23n+igAmhgAl5yZFyi/LgqX2rL73yil0n9947kXXNFa16OTtzaO8a14jjltHM9DtBfm5xClgcWu23l37TC/oOiKWZAbsyjcGs8mF1CvX9i4gagWFWKhgsrEVw+jDcygxUuobwO0iLCKEpEFkGDNNYHBkZrdS/9tlfmhk/viPdCYdWbt94mzN/+I3N/fMbUnMnXuV5EeKVvXsvePWr91Ye+dxbEIqhU6dmUoadyqYdY2jq2MErwiic8Y48fI/6MfbyTBJCARPy0kJ4huM4Y6dPjDdXXnD118KZicsaiH5y5uBesaK/aLSNWsGr1eOCbJjlQ4+5a7dtX1ves2u1P3PCyAYdGE0f2dhT8m1AljsYdB14M6eQTtvIhQ2YrQhWLGG6SrwZB4EZY6g3lzn4tRt+OqzXejqL86K3WMBM7ejbm6Lz2jgIs1hsOiuGVsRdjbnXjv/L32yOhb1aRkapsljzBsfWHk1n01G4eHpFoVgoG9LndESEUMCEvPQwDJiOkubEqZMbRlZd+NDIYP9ODPb+u+m5E7nTBydQWsxaa1etRccK4UgPp/fsxqoVQ8bB48exfiALJ2VgYbYM2QqRt100Gy2YcaCSb1rJPYQhI5gyBqQBaZswojqmdt6djpdkOuy00KWkjMUF+HVpCeF3WUGITGDANXyjXFu4yEpnL4KTRq6rW67dtG7c6Ok91qjXMrm4Y/Ske5uZdeusgw/cVzyWW1l789YRyTNKCAVMyEsjAAsjatVrk41G4+pmc74bUpQH+voWw65SbmrmKGr+PBaVRHvtDOrVeZiRjdCK0eNY6FTLSAkbecdBpdxCqLOo5yGGWlIRUrp/shEp+UI9FmP61BSypW44HYHOQgOlbBqjg33odOpYWJhGXy4FPb+9QIis6CDoRGi0Kmiq57eW8mL+1PFRz7LeJaW0rMDPBgKXPPp/PvmzmW3XfPb6K666E8krEUIoYEJeEuT9FcNjB2rV6huPH969btuGDYenDuw9ms4XVmYMSxiNBhoTJ9DfPYTqsdMolXoxXz6FjLJrFDfQigSMOIYVegijQBsYcSeAX/NgO1bixEw2h04Qw2ibyOdcNNodiChEISVQnTqJdruNfMqGrLURihilYhFhcwlCJWpb+Ttv2jBDG1FzyW22am6pmENYDxBVa6PZFfG77MrRZuXoN44pMZ9WL2iqJby5cgpv6loZ8/wSQgETco4mYBErcZ00Dx05HrTxSmQH/7llp9uGkxGBensNuUVYsYd2eQEF18Tp8aPIpV2IgotCzkazWYepNBe3AwhdqVLF4CiSiCwfhoxhp1z9GjCFodKzQNTyIPwAaT2qt9VGrNJ1TsVeKxYI/EhJ24DXbKrnpZLvNZstlYI9pfFF5LpKGHFdoNlRe2bDVs9zUmaXZfjvCA48mCnvfvB4UOu4jTAuD7vZmeb07rszxVWzIlNkMiaEAibknJRwsG/Pw/vmZ0+/N2i1huxiqWnUS9JNF4TKokquCwj9tgq3PhwZqPshpCsRmyEyloN6rQrbsNU6al0lUVNXjPYlTDtWX6tAqh6IVOqNdBu1Sr5ZR8lTpWVbqm3EEiKMIAMlX6VZI5RJItYytzJpyDBGPpNFPp+HbZlYKi8hVAnbLXQjblbROnFEBAtTY/VY/qTn+bEUtliKHd8ZWXPooc995jM9Wy//TOw35g0nRwkT8gLDKjiEPAO6pnIojyfvk9rk8czpiYPd6rE+taQev96WCy65L/Ra4dSxo2/u7u2tzy/Mie7eLpU+GyjmsoDfQVYJsJS2kLYEZMdTj6nUG8SIvTgRZuRJJVKpZGwiUreW6cDUiTXlIFIp2OvE6LQ6MGWYPM9rqASspG0pEUslZ5XFAS1ita6IVKxWYjZVmjbV923l8Xp5HnGrqRJ3iFp1HkuLU6hMHUN1/ADchWnTrs3bDtpW2jWdVqvVjMJgemjlylZYn+AvAiFMwIR8f1iqt43Fif2rZw8/VurtyU97U/vrzaBeOPzYvrfs2/3gRSt7Bx6SC6duR8+KCd0MDbRapZxz78Lpk+8cWFGqGwZkW4ZCRj4yzThpRk45DqoqxLqmlfRs1rINlAxlbKDV8JPPw4balBRArNbXzc8RYhhRhHQhg1zJRBio9YQL1zXQVr5VgRWx/nigUrKtkrEhlLgtQ7k3QqfeTIYwpdTrBfUG/GZbSd1Qjg7gaTE7FgrqNaThoKk/CBQKMiz1TKf71929dvuOT/e85gO3qf1u8reBEAqYkO8bnl9T4kLfxOHH3rFgWxODPV3Hsz2l41ftuPDunQ8+1Dt54shPLp2aeP3A6tG/Vmn4bvWUcHCg9+HDj4x/uJyNjprZdCPV1ZOvT02i1qwgoywZhTHSKRtBRyVfU/dWNuG1A92qDKmbjw0lU3XbUSk2XbIhVFLW13GFepfqpNvdX4Rf99S+deA6BuJYwEqrlGzbsNQGTZ181fphEKq07Om2cZV8LaQsC81aDbaSrKU+GURqZ4s5JWvdZVqt22xG8LIZ2P2r6iOved0Xs5uv+HtreMdjSr4t/iYQ8uLBJmhCnoLBnoFYyqDZU8wJePU3zE9P/MT937rxt049tmvFK97wo/94ySVbPxHH/vCxPbv+272f/vhHUTt26cimy51Oq7pnobJodPX2TtlOBj39owj19VjHRcfzkMrYkHL52q5eWi1fN3Mr0eol0MN9k3dlrpBWSdVTSThUotWpV6VlZWonbSOVc+HFAUJDyVqlWTOXQWQKNDottb6E7/uIojhJw+m0SrhqHUMJuTuTRtFNoy+XhaMye1DtJNeP89kcunv6UeoelLWKPzR/4tTF6JwYVfvlqsVUS6Y2capX+kFB7XumOn3M1sdoZupEWn+fvy2EMAET8oIhd58UWJuf2Dl+cHGglJ92lPAMWTJOH9j7kaWpY8dHVq3+5mWXbvr04Yd2frgxN/krt//FH//olou37VlXcob9mZNbcvmsV14so+jY8HXq1MLUUVclXDdlIO1a0HlXjzQSSp6BEqZpmrAsqaRd0oEaMg7hGpZKv776nrqNfPUMO0nK6VIBRqsNw3H0WGT4ng9h6FSsErMpobduOSZslaLbnXZSSUv9AAh9nYyNpMBHXqVnaSl/pg20ddWtRrnY6zeuc5vzq+dv/tyb3GL3LmGlFxuVRo8hjMH4WKZSDTrV/PCKh+P5/dOnxvetmV06dfLIgze1V6zYeCo9vJqJmRAKmJDvjhPHj9u9Pb0S+VyMeDoYGRv55uThgx9dUcrLUsptjW7f/sD84vyKhQN7f2m2eXewafVaK7Jy1r6945sP3H5ic1chL0WlKuJ0BtbSLMy0nVzXDWM9pDZCFERIKTG62rxKuo4tIIVuahZIKUlmsym4Ss6NRgN2WqVk3a8qCFUoFrCVYIWuiqWSsx4jbFkFNCo1JdFU0tSshW7ZZrKO7tQlTEO9boB20Nat0fDDEGYolPQd9fppCLX9euij5S3BCzzUjrRQmzpZcvOFi1qmdVHHdn/IKpTCSrVhZHM5ke7q7vSMrlwyyuLgIw/cF7S82O1duepgpqfn3tCrt+RstSMGihw7TAgFTMh3QRyb5fmZPmH0tDPZgWbfWG2yujBz70J54UdLKqA2Z073r96w5kRfUMtOLJzctP+uW7F5wwZ56eY1mJ2ZQvnUSeG0WliaasNWcrNtPdynAWkoKSoB2oaudKFEacZJB6tISJVuZfJ4NpdCNpNGoJ4vowiWsJWww2V5eh7SuSya1QZcJXctbV8lW18l2oySu6OkLGOJZqOZNHFblhatSLaTXGNW73RHSVuqx5SlYVgGOn6w3AlM7Udeid8wfcjOPNApK6HbyJkZK6gvWfliEU7BgpuNbVGfze95+MERT6aq/WMbToftRmd21p9x8z3351YVKF9CKGBCvjumPHSGU7Jz7MiBi4WMOutWjx5eedGVnzt6+02bmu3aG/2F6TV7jh+4ZLSvkO2z9DXcBsZ37xZja8YwVCqgVw5gYv9eJbAIGZ1WlxbRpcQXBZ3kOq8ZCyVckUg1FpF6B2r9Aam0SqVpF3EUJLI1dFXISCads0zdG1pZNFSyNFU6Nh0HYUvtqOertOzAUCk3UjI1deJVt6lcJml+1k3arWoNqaR3tQ1TL7opOo4QeGp/wgAZ20ZWJWK1leS1Q3hoBr7ajoRr5RHXHFhxC7HZxvy0j047RNEpGV0rB40mvKbhOo84hZ5d+WKXqFenXHBqQ0KeM+yERcjj2LFlLcZWr58v5oq7Ws1m6cDBA1fNTc8Orr/00huKvb13NwPPnC/P9R09sD/j1crYsmYl+tNWMpa2Or5fya6D3oxKo+0lJbFastgq3lqGrTWoRGiqRCqTqlVxpGRs6CFDgJuykqZqLVrfC5av+YZxUgFL95eGHuurEq7taklH+hG1vkiqN+uUbNr6+QayqYwSp60+WZsQSva6y7P+lJ0VKsHqXtBKoEIt6OgxwgbSWspqnU6tDluqfdJilk1Y6QBOJlCCbyKqnYI/8RjEyYMo1mZRaC+IaP5YKSubpcapqSvaJ2ff1ZidW41mmHrs3v2Cv0WEMAET8l2hx+wq5qWUX39s951jUzNTr5726pvX9veOX3TNtTeOP2RcZS5Mj9TnFsTM4SMYHeiFpXsghw2Ul2aSSRRiWw/9qasQayYJVBfIiJV4De3EOE56LwvHUAK24NhBIk/ducoPAix3hFYZOEnARlIrWstXJ2ddpKPTaiNluXAsC4EebqQeV6EWtaUGpJDIqe24SsSxStJa/JHvq+1o4SvBqxSti3pYKUfJ3IanO2VZRjIm2Ff7lFZJPJn8QSwnYn2d2pbqg4Lah1ImD9NKYdELsTA7q/bVWte/8Zp4aNuFt4qunpOhh/q2HVtYMYsQCpiQ71nEesjsscr8sbk99992/UMP731vyQh7t2y5cMGc6iq0T50spJ0MJo4fQU6t6hpRUuXKzmaQSWURtANUy0rCKq+60kiadaVKuYaSaKzSZ6xSqLCVMHPppONUpP4LlSy1/pP0q3tIx2e+Vsk51tWubFePSFKBOIKjC29IJMOZ2s0OPBWlU2lX3bcRK2GGoe7klUKjFaKpRKuLY9lplY7VOoHU/ayV2NWHACedguMm0yip56g0HWWV9k0Ytq0+Cagnmep1HBe+6WCyHaFiufWWW5zoWbGu7PUUH12IW/vtllMexDpeAyaEAibkhaPUt6ah0vANU3tuO9g8feztB08cuS5ba8RGW8kqNjC6Zj1mj+yHrnrlq6VVbaCnWEB3voRiOoulqQUlyACxCpSWsqfrimTeP20r3THKVVKVUCk1XJ56UOgqHTK5+psIOFBStFJKgqapO4mpNKx7U8dJerYzKXitFjwZJL2mM7lUcu1X14iu1VtJE7dhG2ipbaRLDkK9jZSl5G4n449tVyVhJeB2bQleuw3bdtTPof4sGC5aSraREm/Ni1Fut4CeXLPrgs33b9l48T+XLr76bnQPtuCuWlA/Rlt9WGHyJYQCJuRFScN6EO9e6c9Prp88tm9+z4Mfmdz94EVR7DsVvy6gUq9f85PrqXr87sJcGYVsKhnvOzjUj3a1jUaliU7TR6fjJ0OEVKBU27WSa756CFHKEYkY220vmTjBsnRzsJEk4FjJWSTFOpBMyJDM2KAE7fttlXx95Ip53W6snmMn67aUfEOVkrV8XbUfoZKonU+rtKvStq41rYdAqWSrPwS0KzXU1YeGTkvvhy7+obYZJZ8OIAoFBPk8xrZvj9NrNkyFg2MPNkS+On/4xPqgfcIQ8oGG+mkax+67daI0dsls93A3RUwIBUzIi0BQqMa9I3el+0dWFFevc9rzzkbHz2cGVo3Cn5lCsDCPxVOnlESbiOp1BL5KmzmBfCmX1IJu1ZtoqEXnWz3toK5a5brLHbQCP9ZVOZYlq68J27ZybaBz8PJUheobuul5udlZJCLutJpJE7Ll6HHAIinK4QUd+GrREynZrgkn68IpLE9PGHrtZB3bTcNXzw28EG3do1rXotbbtSykMi5K2azumo1Un/rwkMooLVviyJETo0YDHxwa23w8b2YWIiOyWh2/OxbmCSs2/zKdTZfVnvv8JSGEAibkhU/CWVfP/7uQW7vts1ahNDM/vvf91cnxK+v1xW432y1kpFJjbCNcnMVSYwkdLUmVMPtyeliPhUxeT6IUJpWrIpVoA5VMAztI6jnrzlmxnn1QF9PQ9gxlMrxIWEbSGUqje0knCTjQPaBVajZNJXDdhB0qOS8X6tCdr9IpU49iSiZxkMrYQeDBa7eQy2XQURKO1X3fk6g32iroppDNFWCaKdhOGm0l/UpyfdrAUs1T4s+iGbVis29Fpzzf8cv18YyJVLcZmQ03mz0EJ3XQSHeqqXaD14AJoYAJeRElvHy9c75yZP/NuVWYM/KlHWFt4dLO4vyKyeqBHjc32B/FZqYTSTMwfMT1GuxGDYV0OpmZKJNLK8kuz1gU1DpoN32IlJEMK4qTalaOHmOUNCUHfpikY51+3ZSDMNTTFGvdHWQAACAASURBVKq0G0fJ93VPaF2qUs+IpIc4eWp9PSzJMgXSuRQ67TqESt7qgWR8cKRSMM7MuKTyMHLFHGKhtisd9WHBwHS1iZa630nlZBi4MujY1bSbmmhH9kHRFI96ueyeyEiXdUfugYH+dt+q4aXB0VW1bO/acHlWKEIIBUzIi0xp/ZaqSsN3dCHe2Zx4eNhfrGwa2PaKDWjXDNNfspqnjr6qcvzQ5eGRA6XG/LTwGx04SpQ6seqmZt0ELfSF4FigE+jexoCt0q9tukknLC1h03LgeR34SQlKK+mUFQVBUshDSguRsJWb42SKQdOO4Xc8pFw7aaKOouUhTY5toa10q5O1biB2zOUiHUZGi9fGUj3CfLONsk7Vhd4wLvRXO6nMZKq768jKjVvuHFl3wSO5V71zn9pU/UzPcEIIBUzIOZGGG2o5vDS9NDE7ufdAudNYLzrm2NjabUfWjIyOhF29+emH7rfqM6fhLTWVEH24OpUq4Qk/hmsYcC1DyTTG8nwNMQypkqxKyemsSq26AIeWpzDQbjfgQNePNpDKZ1Gr1pI0bbkOLD2+110ea6ybuC0lYtvRxamMpIla76mp5zZU8o0sFx1poNKRWNKvv3ZVua/Qf7gC84H+zRfdkxkc3dOz4YLFzNqLyky2hFDAhJzTdA11eSoRH5nZ/9DCrodub0Xl1vCJysJEj4dCbtWG4XYgzTCeRdBpod2KkiFC+tqu8CO1hDB0YQ4RImX6EMrEttLmgCgmRToa1U4yFCnW8xy5KQSBj9ZiNZlyMJfLJrWdkcx0ZCZjjV2VrHXjtRdH2ucwHT3syERgOAjMNBpmCkuw2lG+OGHnevauufSab7oXXHEP1m07JkQPZzUihAIm5CWXiPXNklpul/7CA+WH7loxu/e+t842Oz8R5Xs2m6E0TK+VTMjQ8kKEkUzG8+phQaYlYKrk60W6BKREp+PBVQm5K+uq8BvD6wQqzCqpRr5KtjY6fgfdxawSrnq+HqoUG8nrSyXhUEQI1bqBrdOuSsupjNp+Fs3AwVLk+rJ3ZH+QK93Ys2HrV1dc9foDcvjiKpMuIRQwIeeHjJ3etro5Ihd3/v2pW241xyvN387FRjbu1NEIOrBHetDbP4JiVz+sTFaXvELYrKlkOwtdZzoT63HFHlrtJlw7hZxKwrooh66V1dHXgQsG/HQaHfUipmsnxT30xWF9rTdW0jWyDpy0elx9L9PTo/anAMhsEMa5O3MbL/x9d9PVO90V21Ta/RWeLEIoYELOQ7rXVzr5nYdrbrodB0FWqPS6ctMGDG+7AG53P8xcN+BkdW8pIFA+rC+hPnUSi+MH0Z6dQilowvFbsFTidfSEC5atUnCcjEyKTUslaaXgVApOPgvDdfRg3mQsr74GrJutdc+v2FIp2cojRN6zswN351/7k/ewQxUhFDAh5zXl8T1yse1FfjYfxUqUl1xxMQpDAwhzRQRKvnG2Swk0i0iXlkQAc2AU+ZXrkN90AeZ2P4Tq4X2IG2WURIxOq6FcndWZVyXarBKri+7+QZjdRdjFAmIhkubsSMm602kjCjyYQia1nmMzCy9MdeJ01zyWK2ESQihgQs5fMkq02e5+vzA4Eq8d6oXR04WKSrL6Wm3KSiEQDqR6K2oBh7EehmTAymTU9130XJnTPaiwuPdheCoJp/IuGgIQ6Szyw2uQ6htSwu7X8xki0rM3mLZ6vpUMd5KBj6C6hObSAsJWHY6VU9u32tLIVHhWCKGACTnvcZwC8sVu+CMrzDAl0FIC1cU29FSBcduHa4aAESI2hHpDuslMSYGSrnRMJVIbfRdcgcr0DGZOjiNlSrjZDAbXrIPRO6qWQXRgwM6U0Il1felUUpxDDyMWlp7Ttwci1wV/YRrtagOh4XilYtcSJ08ghAIm5LynDd+qNxu90jTThmsjFEYysQLaHZiWLoqRTmb903WaDTMHyy0kHa2koWQsQhjZEnrWb8HhQweRMiQu274JqYEhNO1sIl+pUrQ0UkriNoRKzXpSJUPPgmQpiYsIZsFA1lbpGrNYLLdiJ446PCuEUMCEnPdUK/V0Zaky5kSBq+cE1kj1zkvp4hmOuVx4w7Zhq1CqhxGFQQtWKp3UYpa6xqRKw/mRVYlwh1etRHZoNdp6Pl/DScpPmqadzKjkpBzYSfqNEwHHetYjqRaVrNMZE+6Aq3Q977cMcJwvIRQwIec/i+VyplyrbBzpzlltv4NiNgs/jhJZdqIYX//G17F7/0FcvuMavPp1r0eu2AUv9CACE1YiUgtWtoh03xByg6OoKIkbdjKXYVKW0jAjJXKRTNQQhB3Yjr6mHCeVs3QVLFMl5DBUb/ZcFzI+6qFpl3lWCPnBY/AQEPLi0okaRmRE6abXFnpmIj3bkB/JpAjHZz/3RfzKr/8e/uEf/hk/8zO/gC9/5avJhAzCNJPruLpetP6c3GoHyORLSrwu2r6vBB4k1bBCJWBTF99QqVfPIWyoJQy1lA3lXz0/sFBpWKjtWUrkBvKlnsXBwRU1nhVCKGBCznssaeqZi4QMQolA6bUTqHSrr/lG2PPIXhiRiWI6DzM28M1bboSQHmTQgCn9pByloUzcrCyhuriUzIIEJd/IbyXTEuo3cKCEvLRUwR2334VbbroJtXo9mWlJjzOSsYSwzGRGJGm5cRDF7VAaIc8KIefA3wYeAkJeXLK5YmDFxlzU9qWMLCVY3QlL93r28LpXvhIP3/swWo0G+vJZvP2N1ylBV5WUpRKwtTxaN4qxcPIUWktlWJEyt9QTK6iEbCo9mxFmK3P4n3/2l7j5W3frSpV401uuxx/88R8hl8ko+eq3uEymH1QxWFeF7iTFogkhFDAh5zvFQrHp2u7R9tJilEnZZhioZKuLZSgrbt+2Dr/7W/8vFufLWL12JS7acTmas9NwnTRi04WhErNuij64ezdOHT0O74KtsB2pQrCApb9vpzExPo5777obtmEincvgrjvuVMn6Ubzy1a9GqJ4cqYSsk7LnhZFtOwuRnmqJEEIBE3K+4zqF0E2nW/OnW3EqMiE6KoD6TVh+C44S6Njqfmzbukql3QjzJ08gX+iCZenruLZ+CJXZBdx+yx1KpA1Ul2rIZhzkimlEbd2Z2UBfKYfRwSEcn5xBo1LDhs3rMTI4gMDr6KvJyUxICAOVqIO41eos9fUMBjwrhFDAhJz3GGbNiPyW4bdqot6O4egey74JU/dYthxELRde1YBpCOQzXfBVYtXNxrpmdKvRxL999t9wZNduWI6F8QN7law3oqP+szMpGDLGaG8PfudXfxE333ZX0mP67e98F0YH+hC3m0nnK/ViSsMhglZLem2/DcNhEzQhFDAh5z9x6Btxq5pylXUtJd24EyF2lICjEKFhQj2qJC3gWCZqC/NohBFcU6Kt1vGaLVwyVsDaD7wepyZn4c8dRyUXobe/D6bZBWGEkOp5G0YHsPE//QdYqYzyawrCayU1oe20knRsqTTcRLtWlaE09AxNnISBEAqYkPMf0WiIlC7CEQcoIkatXoOvEq5frSmJWvBjiXKniajTQsaKsXJFH7p7SsjChF20sKbUq9YbQSw2Y6lSQRT4qNRn4CmXZnoGAdtJqmspZyc9oHMF9Sq2BVMtshXBl5ES+f/P3psHS3af12Hnd/fb69vfm30Gs2CwgwBBEiAp0pRImZZEk6IVbbYsS1EsRXKqbFUsl10upSLLlpKUYyuVxKrYFWWpUlK2aMniIkuUwAUEiX3fBoPBzLx13tLb7btv+b7v9sBO5Y+4ygQJzPzOoPH69XL73tvd7/zOt5xvgiJJi9K0p9CDGDQ0NAFraNwMKGvDSNPEsqtMteoctq3EtzmYRgiTKYI4QDIZ4fDcPG49vYojczaqdAykFalbgwjWknA09/fOt0ndmg4WF1cxiXMcHGyhJiXtdJp+YM/poIiIYxV7UJsyI7jivuA4gms4qeq4I+0DraGhCVhD46ZAYRaGQuWbpGGNIoNNirSulPTnFlVK6jbH2qEFnD9yDB2vQrC3A5fDyMS9luegIuVclPQ425D2I7adtN0WujLz18HuaA9FXsBIaNudObqtLR7TeSpmWfIld2qFOM9Dol5twqGhoQlYQ+PmQDYMzCpLF13UxKklDHALUgWiVsz5FsrSwdGVRbhKCqXgeSYsYsqqqlAxsRqmECz7QhukXR2vI8RaVRnatovlro9UlUTSAVRcg6gXjvJhKIsu3ICkZMKSYVoj2zYD/Y5oaGgC1tC4KWAoUxllbaqyhKqIIIk8DcsAFyR3bAWekKSyKeKoRMuzUBc2oiRGr9PlWUco0xSGY4p3dJplpG7pwjlfulRFjDapYJXTbY5Dipm+1A7PIsyFwA2TaJ5+rUgBm4Y5guuH+h3R0NAErKFxc3zJLJfI0q4a51eFvMjp/wqOacDiXt8qRZYk8C0H6TSAbXbhWTYRL7cPWY2vc1lBkWq26DpJZlLQTMqubKcgIncNIl+zIpWcADk9nhS0oWxiXqLwunGczbMiI17WPcAaGu+Uxbk+BRoaby+quDDKovaKkqfzKjHHkClFykTNZJrlpHoLJGlEZFwgJhJWrF7B4WQlYeeK7q+yXFytuOiKbhDjDkNC0ZUMY+DSKu4lVjLWsMb1f1XdjDk0TCu0244mYA0NTcAaGjcH6qI0iAA9yzBFzZqkbhsS5oEMRLxxQqK2RJGzwrXkwgMWTJMLqTIhaYP+sdrlnHCV5+IPzUq4ootpmEK0SZpC2Z5MTKqJ3GkDTb/RbBoSqW824dAErKGhCVhD4+ZAkSU2KdeOKQVRNXFnKRXOTLBxHNH9pGyJfHm04HQ6RU4/2f85DCO5XhLpMvmahgWXyFWoWGYV5jIpiSQwyqKSEi8QudeW9RYBc7UWV0TXLJUtM83qTJtwaGhoAtbQuDkwHY+dLAo7XP3MYWPiQ1hEiHmSwFZNZXNJRJtxzpduZyvKlNRszmq3rEk1mxJezkgpsxouWBVziFpIOJNQNY8dtGyHm45pG/R4S15tVi1dyjQk03bC1tJKpt8RDY13BnQRlobG24wonJpGXToG52WlkEpJGNlzLFSJiWmcwfRNKchKeFawZwjxmkSiwTiA6zgoOAfMnhyeQsHka2SSJy64utmzSQGzKm6GN6iCe5UMHgYMZRp0X4G8SNkNOlxYuCXV74iGhlbAGho3BYgyTcs0bOJLUqZElvRPmaSELYV+pw/b8BBPMwSTkIjWQBzTI9IaeZTCJCJOgimSMJY5wsSpcG2PrtNWU25rMlFkpKIru6l+LkheV0TwWUEEXEq/cclKGaq0LSvPa21DqaGhCVhD4yZBPo1bDuAxmSLLYbHHs+HA97pQin6254kzPVhul9jRlPxtTtfCLEJhFMgUqV+rRqEqTKMQcRIjz0viV3psZqIKK2xf3MJo6wB1QhKYWNag7RDnC2kbSnFou0rzPCulHUpDQ0MTsIbGTYBgOuqoMnfbtgXPsIiEiQNzUqqljV5nGQcHCanhHnEz9/z6qA0iYFauHEXmULJrEBFXRNI1LMeR3l5WyHVuk0Ju4WB9iD/93Ffwp7//VQw3iYTpPk76KlLCNeeESy6ENvK8qCJXftPQ0NAErKFxE8CxlGlUpVHlGRwiV9dw0XF7mPMX8cTXn8PzT7yB7asD+FYfVU5kSwSaRLnYR06mMYIgRJYV0qZU5wplVCGd5HCqFpJhgRcffRkqqHDl+av4sz/4MqKDEGWcy8wjJmCLq6FrEtKmGSo9CUlDQxOwhsbNgiRLvNqsiQcb8w3f8uEpDy89+RJef+4NWAmwfXGAjQvbqCMTHjpEsvTYyiOS9WGUdMkcqMRCNixhxhZ6agHXLu3h0T96FFuv7qBV2Fggcn/z2dfx2Je/BiMhsi4q2NyORJq3KKuyUpUuwNLQ0ASsoXFzoK5rVRm1Y7iOVSn+XcGzfeRhjlefeQVOYaJNZOwXBvbe3MebL6wjHeREsHNAaKFd92EnLl1aMKY2EJiY7iR49qsv4JEvfUtIu226MLMabk3kXShcev41HFzdgkm/s/c0D3JQllEZlqVNODQ03kHQbUgaGm+3Ak7TDqLInqtrOLYD23SwubmJ6SiCW1lM0rAMS3p904MEF5+5gk6b1K9jYHllDkGUw7K4hbhCHOQIR1PEITtlKbSUAUUEa3HZVW3A4cdcO8Cl517GytkTUF0XhlWzCM4Nk7W2hoaGJmANjZsEZZH3LNO0TO4Mytm1KsfzT74EmxUq+zTXopWlbcjmycFhiSgJwfHi8dZEXLEMSeMqmSNMlA2flHTNdtBswqE4ykw/absteiBXW+9euoJyHMDwDdQyU7iOLc8ZKdW8moaGxncfOgStofH2wrQMs+so07aJQDteG+uXrmKwNxQjDbGY5DYhxeYaDQ97lgm7IjVb23BLGz5smDldrx24nNMtIN7QVs3DGmox3GhYvJYJSw5td7hxDaOdPXAXcs1WW4aRm6Yd6bdDQ0MTsIbGzQKFJOs6huEoIk/XauHKxS3p1WU7SttkI8lKvoi2wd27pGrzUvye2V5SQKqXCLyZjFRCrhsse7nXl1Q0J5ft2gATvElcbNOzw4MQ2xff5GfI2Ad6SkE/tQ2lhoYmYA2Nm4eAlzqenyexY9ouBsMpNq9swTcMyemWFfs6FzLvty5qNGKVQ8tsM8meWaSSLTbSqJsRhXUtF1uo1SbRa8ElInaIrA26v+KhDBUTscK19V3USUq3s2lHUVX8QhoaGpqANTRuCtSpiiZTn6hSOZaJ0cEIaZxK4VVNxMiDEhSa8PFM7Ip1JAtcvhBpoigLFPQ4JmSZrmAqul4QURekotlXgy8FeNpwbdZ0XwXLMTAZTZDHOTj3bBlm4bdbugpaQ0MTsIbGzYHtFx7h/qOuZ3CoGNi4ekX4kiuYeTZwNSNabk+qxcBKoaRLzl7P9LPiWb+GhVLcsWpkrIzp8QUPWZDSK9bIpbhk8TRC1UwgRJERsRNxI0xIOdMXvVKV13Z1AZaGhiZgDY2bQPxuXFXrr7zWsw1rvmW6xIo1Nq+uy5jesmTqZCVL14k5K4MJ10BKl9y0UbouasdHSsQbVjWIRpFbLkrHQ2Y7SExLHluazXMbEU0knpew6aetlFRc0wvJvhRFbhQZ072GhsY7BboNSUPj7YI3QTg+6FtlPddt9zDdGSAjUmQetCwDBZOwamYDs/Lleq2S2Dkl0izjRCqkpdrZkPIrlsmoSNUaRLyWZaNl2E0FdJILqXOOmHPDBRE2CWxSvZDWJF5lG6ZZlTwLUUNDQxOwhsYNj6RUVZa3iBrbRVEhmIaYJikqs0bEPb+eA5Da5RwvS1ieFShTe5kxXVtErVRHKxMVkW9CZJoXBXjWgp0DfddBn9SwRWrYqni8oSVKmC0vqyyVzDBzbkmEXjskrF1bV0FraGgC1tC4KRiYFGjKHtAWVzKv72zC7Xk4c+c5dLtdrKyuwPV9JKx2SQ2nRM6ZqmVYUpmXSNMMZVGJC0dBJMoTkoIogkXEOxyPsbe5jb20QK9nEwFbyNMSRRxJm1JJitg3alHTTXuxmVqqpZ2wNDQ0AWto3ARQCp7nqRKlMY0D9Jf7OHXyg1hdXEJBStb2XWSkfrsLPXTbbbiWA9N3RLWy3xUMi5SvkiKsirZlOaSKLVuqoZVtIQljlEnGswZhGQbCIMClS1ew8fJFXKRLTCq6pMdXlQllOlHuOFoBa2hoAtbQuBkEsIUiy21D1XZFNHz4xBpyUrAb++tS+ZyPSthEkq7rYhQdwHf4ugPDVmj5HbruQ1lNmFqZRMJGidomRWszoRbwW/RYs4WazTwsC665itUH7wGmOTZfvoBXn3sOQyLpruNXdW0ETq+r+4A1NDQBa2jc+MiziSrzwlNFaZOIxTAYoK4yZHXCbUHiYlWExIkp3WYAmW3ADQ0Szgpxawrbc4lsSRWTirXopyKlmxcVF1TR7fQ4Im+DVHRN97HSVW4LVp7Ad9s4cs9ZrJ5aw5X1K0iLrKpaRmT4C5qANTQ0AWto3PgoixTtVsuqDNvOsxBpnqKSTt6S3Z3pfvrJyrZkK8oa7Dxp1BZILKNMC6lmtpyKCDlFp9VCWZViR+l5Hoq4gFHQb2UGOB6JZEeeV6kchZNL37DRdtBZnUdCWyoUkiopdBW0hsY7CLoPWEPjbYJ35ydMZarFOI/bURaL2YZUR1UG8rIm1atQOg4So0ZumdIXnLMZh+R+pQEJRV6iKmoUaUmq2ZSuI66oNgxbph75pKTdNIdHFzuK4CQJzDSFWRQoifDnFxdomW3mJJ+3Da+rnbA0NLQC1tC4sVHXtfUbf/uvfkJd2/6puxfb3bRg20kgTwoiUVK1RLwLK6tIixz9zhKC0QBVnsG2LJhsxFGWaLE6JjLlvHCeZLBI+RpM1NJqZMEweYoSDykk6q6JY9l4gycrZZhZXdK2HGlTSkzXvayU0gSsoaEJWEPjxsVzL3/N/O1/9uv3vPzqK7/SjqYfOt4+jiSewiQ1a5J6nZ9fwKGTx7F6yykol4iUFfB0io1LlzHaugbDdkkh5/CJYC2bC7kyeLZNSrgAT1TiXmBYBmrfQkLE23I9GcKg2AOaSJfdsMqMiJ62U3Fxlt8x9weDQ5svPz53+LYHxnomsIaGJmANjRsSb77xurm1dfWBo6eO32MN9zHOE4RZKr7MxxZXcObW2zC/uoTKsWB3fPFwdnwfZ7tzuGi8iMHuLtptH0Eaw2cXrIoHLzjgGUlFXpASdhuDjarJG2c8vpB+qqJGkgQyIUnZXnP/VCHPy+7XHn/h5x6/8Dv9h77/B/4ZbWhdv0saGt996Bywhsa3GabhON257uHuYq+1eGQVCalYc3ERQ1KulefB6rQREblyRXSehI2dJBdNOR7OPfAAbrn7LhSWSeqWCJe+oYZrYxIFKLn/l36PQnoOx7OnGdxEoQ5yRPtTxOMY02GIaBojjmIk0xBJOKXbhmawv3Pm7nMnf/rWYys/+uJjD/f0u6ShoRWwhsYNh7m5rpNX6cpoOjIX5udJ5booowidY0dhLcxjPxhjse/BLdgTmpRtYUu/Lw9jYJW7evq0mHRcfuUVJGmGaTBB27AwCkLJEbseqeNhAIcUr2e74N6iaZrK5CRl8tSknL7ZidR7LXpr6M+18Jd+4OPwVw4dik3vZ1IjfL2u6z9USumqaA0NTcAaGjcOWj3XMmyjm1Y59icjuI5JqthAZ3EBVcdDSMrXjwu4Rg5VunBMV1qTDIdNNZSMGZw/dgts18cLj36TnmuT2E3QsX0ptBqP9+H5HtJSYVwnSKoaU1LXORH4cBKgJl5dO7yK2+86D8e34XI7kufQ7Wx0qc7mhfqJyZWnn6FdvarfLQ0NTcAaGjcMwihsV2V1xLSIeIn4OnNz0us7uraDThwCZQaXVHGr9kFU3QxOqEkJWxXYhNIgsuYe4c7CMs7ddz9efexxej5g1iWRtoIiQg7jHDE9JiwyHExDjEkp50qROu7g9OljOHX+HDIi+pbJDh8Jak4SE7nbnmHZMD8UDDYfJBW8oVWwhoYmYA2NGwbR3sHpaO/a6eVuh4izxObuNbiujcWFPoJgitV+HxkKRHmBehSg73aQZhN4ng+QwjVdDkc3fcLdw2uYP3Ece29eJkI1ME1yZHWFQRAiSIjU4whBHuDcnbfBa/s4fuw4siDBpasbOHv0MPIoh+UbqO0CVZHCJFXeabWXqip/33DnxT+m3R3qd0xD47sDXYSlofFtBKlK48XnnvlwyzYXxwf7GAcTLK2uyCxft9XC9nCIYZriwpUNlIaFKE5QhBEcHvcbjOHWRJRphLrMYTi0PnZsHLv1HOy5eWxMAmyT0t0k9XtxPMUmidqTD34An/3FX8DaXbdj8exJDPIYg2iMuaU5mI4pyrdSkKEPtuOKzWWVZbaF+n4nj9b0O6ahoRWwhsYNgT/70u+crvPko3kc+QvzcxiFMaaDIUaDAYyyxsKhw9gn1XrmxEmMkxJeWqCMM5hGhMrKUbBKbZESJkZWddP3680v4o73PUhq9xuS91VE3HVt481rO4i3tuGcOYLjt57H5ddeQjAeYMXlnHOKgh7bneuhJhJ3HE+eU+RccG0oU6k7pwd7P/3KU5//nHLaW0eOHz/o9G5JiaBL/S5qaGgC1tB4Nylf9pjsfun//G8/ZWfxPas9H1GewSLlaZUVDi0uY8RWkZ6PzYMhHBgYk/I90+tisD/Aqm3DKDJUZUoMWcJquUiiGKpNBJoX6K8dwuqZM/id/+P/wutX99BeWcDVwR6+/vpFbMcBPvY9H8ac4WP7yjpWDy+ju9hFyyaJzMljRYRueCgqi1SwQ2q4Ql2li8rB3+h5/mdKu7y6vn7hOdO++tjm1lPPdrz5a735UxNt2KGhoQlYQ+Mdi/GVZzp7F1646+Lnf/sB17VPn2iZn/COrPQPDnbQ9xy0DAP7+0NY3S5KIuQoSzF3aI3Ubwbf8VE4LSR5ggkpZI+Ua5srpZk0SSWbdH8Zx7DoMXE0xT333YvvOxhg/3N/iEEcYnVlBW6c4ulvvQgjMvGJ996Jk3Nr6FeVXBwibsOpSUnbpJodGWnIjMrhbZteq1aZ77ru2Uk+PVtkyccsqxxe2967NLDcZ9oH64/uXnvp8eWV298kIo71O62hoQlYQ+OdoXjf2AG648ULj/zbn3Gy6U9ZdXHCS+1WR8FcXppDNudJodQeG2JcO0DbddDudzHKc2REjOPxBO1uH89evozblxdh2fRldD2UpJjrwpRwdZXmcIh8i5SUsDKQxiN8/Hs/iHO3ncUX//jP8LVvPY1WadJjurj43Gs4Wiuc6dZYu+MEPIMUNZhkTTH5YOKtqpzInB5v2shVCVOxfWWKNJnQtkMVRgcL7XZ3wbbq9xpq+sN5Ur959dIjj25efuorC3MrT3n9o9vaT1pDQxOwhsZ3FZHxurX5zVc/aIbj/2zOyM7YZYY6qWU4ndPODwAAIABJREFUgk/qsiC125vrYo6I1z17Gpf29kgBJ9jb2sLZO+9EzGqUCHOiiBj7HQRZBJctJeOECJIIOMvhtlyUSQyHrSe5O5g9o9MAZ48t4Rd/9idw9paz+Ny/+RNcubqDaBji+aeex52feJ94SWdEvKRukTfUCxTcikSULNZaNrhFyvfpfrOGZVZIojFapNJ7Xd6vAlUdLJZFuWjAu68sis/u7k2fwP7VPx7svvT1+eXbLxIRJ/pToKGhCVhD4zuOcqS8YG/jrm4+OWa5FRFbLlOMDCK+Oslhc39vVWCeyNNdcbC2OI/dYIKuqTAdj5ElKSZxilPHj+LCtR0c82w49ByncNGxHMn7VkTGsCwo7uVVPN2oJKVMUjmeErn6+P6PfgDvfc978IUvfBl/8HtfoJsHmF9ZBNotxDYbezgwPA85KV+P982mrzsJYh7mUDDxM8n3ejCjKVr0uKV5Uu4pR5uZtE1UqoBCbBjKPgLDOlKVxseGw+mFYHrwJ9e2nv5K21t+pj1/dF/nijU0NAFraHzHoKzEUciPhNOBHZcKnowYbMH0O2DDZva3MEnJOnS9LnmWLw9UINW5tIwX1texfPQELpMq7qws4cLVN3Fo7hBGUQyXiNEmpdrqzoO2IqMLmSx5rKBhVKiJPA1LyZhCmCVW59v4mZ/8FN5352m8/srLWFiaR7s/D9vMpcmQtiDmHSASRm0Sj7PRhyVmHrVykMQ5+v1FMQsZT0aIA84902LANkkl85Hy+MS8sas2rF5dW+8t8vTuuEp+LI5Gjw/HV760ufHkNw4dXrxsGKcK/cnQ0NAErKHxtmJzZ/34YDy4u1ukRkoMaRKxlWmCkpSjxTN9HZNpmtOvaLkWLCJgDgmbRMaHvQ62NrdIcc7jwmuv4vCxo0jyHD0uuiItmcYxPc+Fb9lIo4h+WqSmK5RlRSrWlNYk+gWqzCSvW1UG7r33PO698xxyUrNlOCGijun+HEWage2mWf1WdU23NWJVyX66SGm/+TrPdoiiFGEYwUOLFLwnkWveflFkMp+4RCUq3/G6TlkXpzy3c0pZ6hNlVT1y9Wr8LwfDi59fmD+jjT00NDQBa2h8+8FGG48+/Du3bmy88dd7i5274+0B9sIMc0qhZ9WkdE0Smg49zkVZZYBHXzGbiNhTpERNrNl9+HYLx5MMU8vAEvMh/dy4dg0xKU+714VH0tNmhZtn9FQiyCIlxVqLiYbJc4HpPtoPOI6BqkjAThtFWEihls1k73ZQZw5KIvV0GqIgInU4NE4E7loVLFXJJKYKDindlkxe4udyEVhN+2iaBrIsRlYboub5tZi8HduFTa/v8YKAaBtcMAbMW6b6IUNZ909H+/fs7b78L5ZXbn9Ff1I0NP7/oZ2wNDT+w8nXfOQr/8sDYbD7q8qt/kpm5F17oYcRqcRBniIgwgyyBDmp3aIgQiRZaRAJWoqnHhG5tnwJCc/TzwWfyCyJsEqEifEAtx5ewbzrYHKwjyQMWZMiGB7Qz5LUbEaXnIi2ENIzeLtVLeHoMsu41lnUbpyEdIkkg8vTlex2B12extRqiX1lScSb0+OzjOuniFTp+bxfJYfPTRPtXg/tdlvUdhTTtlIiYdp/JudWqy33eb4voXQOZdNT6HhYIccwjeqwYRQ/n8bT3xwNX/tw0xetoaGhFbCGxn80+Vb2o1/7nfdH492/Z1TpxyukZkDE45hEd70W4jjDfkokSMTYoZs6RKAtw27CuDxAgfuMjAq2VDQbcOlyrLOESRygbfYx2d2DTyR7bWcL9lwfZkXES4RbsZmHbXLzLkzOLdecB65IkUpdVnMfEy5XPdN1Jsw8zST/bJAy5kIsz/Ng84IAtdzHgyFUyYZXStqeDFbVdL1NhD1xXASTcHbUpVR18wslROymqUj9GkTYBUyLjoP2qaKLRYqdu5MMw2pXVfqD02CwlOcv/DdEwl8i1Z7qT4+GhibgGwRKn4LvOPmO7cce/uffG+xu/N22Uz2UhQOz5KImUpQ8cajT7cKwUkTVlEjQQUxkZ5cprLSETwq3jBIoFzBtA3Aa4vLaHohesWj5mCMOPWRZGG5fg7e8BIvIN5sGcEkpc063iVMRRSp+9/n9N2GRCi2JiNO8oE169E2u5f6W35Z8bUZKl6cqWZz3NY1GFRsWHI97gw2ZnFQQWXORmMVEzCVfXDTmt1CO90Qp27aL5ZUe0iTB+sY2up0Ojhw5jKKEvDbvBy1MYPqm2GaaBhd4VaqqqwejMPv1jeSJo3G0+7t+a+VAf4o0NDQBv3tRZJBKmXfy/t2Q5Lvlv/CNz38s2t35+32U73eiCF7SDLvPOPRLylYRyfHsXuVXGIwnRKgWfCLNMIvpQaRMO30i0gi1S2qRJ/LykAWLiY9Ij8cicIUx51v7c+J8VeckGisXBb2OS4+1ON1aNxXQRVk3xGo34WybSJMrpTnfyxVfhtl8RhxaCNT0LyMFbVa0s7RPFdtisjKm1za4GppImEPZvB9ZVgiZ+q02JtMpPTbHIdqfXr+HUV2KCh5NhvA7PpF8l8jWlG1VpUIUxrR5dtfyxOqSq6cN07g9S+O/u3vtjeVovP7brf6xrRvpc8Hvh4aGJuCb5p1y3lW761jv/j9QdV17Lz76uz94sHXl73RQ3OemKfw8wXJ3DlGRYkwqMclyIS+TWJIV495wROqWVCixkMl5V+ZFVqGkaksmVoeUa17CND1SpGZTWOW16KcLv6/Qy1LEwRhBMIHr+zOytkVdcpiZW4S4MApExsTE0mvMpMvkx1OPijShx7h0l+Lot5A1g6u0WT8XJXcK1ajotStWsSRnbY8IuVQShjaF0GsifhdLS4uYkhI/ONhHTvte0jEEk4CI10Gn7RPZcpV2Tgo5JVVdwKPXqFUloW/L5tC3c6go8l+aBFv94ODib3UXz1x6l38kpnThhcS5tuHov0ka/9HQ8UwNjf8v8arB/kuLrzz79R/JgsEvmml8R7fMYBEZrRAhdtEMt0+Jsoh7MRwHRFRTKV7a2bwKzvKeWF5B27Zhk7Ll4iWDyKrmfK1HxOtztbEFhwi2JAJk9esSeefTEEY2xXh3B0mYwLYsccFqz89B0c+SiZcWYg5tAzIRSbp00bQ7sdGGQ4qay7b4JrrN4lKuWtqXas4Hc0+UUQtxK9q3gu0q26RmOx1kdFwhtxzRs69cfo2tKXHbbbfLQmB7exPXaJ8sUs693hzWVo+g25kn9WsimqZEzrR9xVEAGy1SyH7Lkb8sSpG2Vg7ytJ4qOP+qMuf++7W18y+8m4w7fvh//vBb1z/3C1/H//jrP2z+4t/7XPmFlc/KbT+w+3v6C6OhCVhD49uFrStfO/HS80/8fJEHP9syymU7S1APxzAnEywT0XWICH0iOiZHJhj2W54QeQ6HBwiDAQ72dqXt58jyGvqdOSlkqkgNtxfmUTMpiiOVQVzqSVtRzSqYVG7NZEzKOiMFHI0mUqVMGhkWEZvVasPr92U+sHxt2d9ZSWZYcrvKaDyfuaCKo9m8GOAyZYNdtMT2ctbwwP7PVuMRndNtlYTOWzDnegiqDGEe0zEVtJBYx/LKCpFrjjffvIhJMBLryn5vHqurR9Fu9VHktRRsxQn3CTMBO5hb6KNNJMy7WMvkB1LXILIvjTgrnS95rf5/vbp8/jn9KdPQ0CFoDY3/F65tP3Hihae//stlGfyU4+T9qoiRVwmREhFjFmFAak9x0VOiZMxghRy27WOe1GerP4fYUVggIgrGU2xc2cSeM8TxY0fhEImmkxitXodILYPpcj/vFCapYbGMZOtKG9KH6/R64hOdTSMorlImJZnFiShbh9Qqq1dlNSq3YEMOoxQSNkxLCFfxGAZSz0zQXOWs2MDDbIi7NppiLaFuLnCuShRZBkVk7xHBBslUBja0uz3YjovBcEAEm0irEi8SOCxuzxYNHMouyhQpnRcuzKqI3JPEEyXMvcMc/rZpn5jEDdPxbaf+oSA8wN7g1V9bXjj/rP60aWgC1tDQkLDzi8/8y7uffvSLf9Myqs86VtHhwQeKq53LBEUagiPIZW2SSixklGCW5kJgjunKT4/IzyBydoh0XNODRYy6t7OP9UuXZTDD0WPHEA0DuG2fk7VEXKlUIBvs82xwny6pSM+X3K3X68Mi0syCUHp1DVPJYIWS+3+5etkwZ2TbkGnFrlh1KYYdBtcLkOKumkZdKLORo8poqqGFyc3GprLm59ENecwmHj10aP/DcIqO3xXjDW5rSmnBULAPNVtUspuX4wjBxlmCmBRzqcom3E2vn7DzVpiQGldSqGV4dC48b9Y2VdrdlvkX0mhgDoav/MP5ufNPaB9pDU3AGho3N/kajz/yv9+3v3Px77t2/smWbdk8+IBVXxhFRLQJqiKHT6rPJXIrgpiuk4IkkmkxudF9rP5YdbpeFzUpRodU55Ejq1jpdrF/sI+D/V1EozEOHzkKm8grL2s4pDjDUUBqmpRll5Ql54azXJQs52qlbYiUKCtZ1/fEiIOnGJX0mJJew3B94lFDCDslMuSiqCKj/eCeYZsNNkgJE6GXYjZpoGkbqqUPmUPGzM9VndH+KAT7E1x89kkcPn4K/fllaVXiCmmeQ5ykEb2uIQ5ZYqfJ+1AqpGz+UZRStEV3izLmPmVemJhE9oatRF3zK/N9pgyUKDw6jT+QTPcQWPg1uusp/QnU0ASsoXEzkm+xaz/8R//Th7Nk/CuOWX3Ms7m6qUSaxEjTVPpsMyKtiFQgW0z2Wy0paJoSObscXiVynEYBOr0eJCTLU5FsC+2FPlRewif1113ooXswwP7uLja31uEOfLQ6XXjtFuyOL0RIHEdK15TpRyxRxaXKdZDGiahHvs8WKiPiJBKMplOWtlKQxQrapY0oemBO+224lVRac1GW4UIIkr/pXGIl+WY6RO4VZkMOztByS1KnclHtT/G7f/gv8L1/8Ydwyx230SKhwDQIhLQZJh2v47jijMXFYwktArI8bVQ4nY+Mi8Qk9MwV27bknQsZz1BLi5NjNsVZnudacVJ8IpoGZTC+9A86vVPPKZ5eoaFxk0HbxWnctNjafsp76rGvfE+ejn7VVtlHHaPkzlYxsciyFDURSUkKNy6adiNu9Wl1OqRUfWTc1ysjCA1p7XG4sInzqmZjaMHqmFUok7FiZUxqd2FpSUh1d28Pg+G+WD1KkRT7c3guExMp6VrCt+zbzIVVYgVJzzekcEo1uVf2Zmb7SDbR4FxvwS5ZdHtOxBuzh3TJtVYSuubpDkKgHJqm/Sq4OIsdtbgSmtcaRQ3bcmGVRIzKxdce/hp2Bgc4cuoY7auJy5tXaBGSwmt56M8toNefFz3NU5TGk4DuS7ieTEieowYcKne4t3m2j0zGrJj5dw6f83XeZzpGmzj5ljwvu2WRvvwbv/lPtFmHhlbAGho3hfKta/PrD//z7wmDnV/tuPWDZTpVyqxF3VrsHEUqsyhSUokQBRqQ2uNJQKllSHGS2W9jMp7IMASPSE7FoQxOYKJtzCgsMcIouQhZbqOvWlFhbWZWMRoOEMYxKdWsIXN6IQ4du3P9WVWzzBKUoqeSLSR5DKHZkBdvl92vWJdyGRibbdjgEYU5ESmbc3D4mI6RFDwTrmLjS3bzIAVs1dzvWxDRW6KcufiLw9k1q3W6/sC99+DLzz6Ba9vbKBT3OOfStmTxMZOi5rw0u1hWfFKY1zmDTDcU9HzHcuh2R4quHEeJYi54lGFa0eLCl/ZlJuSMSJuPhRYvLi1xPh1M9oLhweV/OL94ckt/MjVuJuhhDBo3HdYv/In3pX/9333/aLD1X7lO+aBt5qquYyKPjARjIT2+XLgkwtFojCmyJBNzCWn18RwoImBzqY+hKuQScqsRkWOWF6QOY2m3LYvmesFuHDZPSTLEiGN+aR6Hjh3BoUNrohZ5oALnkdNp1KhXVrqzCUScXC1ESRqSV2XXKyUGHLXkfkt6jMXkzgKYVDuPH6zoEgcBKiLPJJjIOEEensCWmGxdbTMTssc00ze7dRW0j9mUHpPh+LE1rK2uotPpkSpvIYoSqaB23VYTfjast9Qs7wMrci4m45B9WWZSKR4nU1owpGK5yQsLDsvzAIiaq784Vy5ri2YiU1FEvXbL/MtlPv2lwd7VFf3p1NAKWEPjBla+n/9Xv/ahPI//jueqD5R5qkbTEHE4kpCt67VF6bHKZMK06B/nLquswKvPv4QdIqazt5zCfK+DLpMwDyoYBhgT2YwnA8wRSS20u2LPyBaVPis/7rnl4iomUQ5P07Y53FvPQrQxKWAurIJTihKtZXCCkgSRuFmZzUjA67lYsaJ0bJluVMi0JVNUqt/pIptMhbiZ+GpuYWLvDpHxpbhqcaUyK22LJHLN4WMeKcitSkSSZcqDHGLcdudd6C+vwO14pMA7sGmR4NgeWnS9qZ1W0tfMYfbZSZUfTLScU5ZoeG7JsbFi5lA5G3rwfrc8V6ZEsaLnUD3bWeZ52Ked+KtE5ltVHf2vhmqF+pOqoQlYQ+OGIt/Y+OLn/tF7ijz8Lz3X+GAUHqjxeB9JFJCCCyVk6rf7aHe7RJxtyd8y6blEIm3Hw/bgChFWJR7K23vXsFnnWO714fc9pJUSlVeS2synAfoOKUYOMJGqZCtKzu2yrzMTplE1+V3Xcxvyov8mo7Hka3n6UTgcojU31/ht8HxeVrxMWMqUYiwpfOIQLlcqs+1k3ShR5kNucWIyV7OCMJvUOoeaE7asZDcu2n/w89mSMm8GNmSkmJOkwGASoN2bw/kTp6G8Dq6Nhjh6/BZs7WzANly4lkfPbwqqmhGGTVEYK2FW7Ryu5uKsZlaxDRb2cJr7kyQlVd1uBljQ44q8kClNpsHnhovD8sN5Fv783rUrB/T836PtZfoTq6EJWEPjxlC++Mof/da5PA3+drdtf6yqE6PVJgKw5pG0XSKEHinRFKPxBGGcYmXNgssilP2a2Y2KyJlHCfKlM0+kW/mYTEZ4c3sTc6Q8jy4to17oII0L7E9TUZwdy5bhCWacEdnYEpLmSmXL5uIoJsJc9qvVbsn8YCbDNIwQ03PZ1INJ7XqvLxMsWzsyX7PnNFtMFlKBXEsumB/FRU9NeLgQ9y1un7K5nSmj57HTFoedme+JAPk6K/SMCDPMShzQfldeHyVtu7O8ho2DIfpzPbi0TyvLh2ET+Zo8pZgdNyQ0X0v+uRRCxkwBlyhRSMsRF6Y1x6TEKzuOYxnSwJ7S/LuqDSnm4iiAx8ocXGWtzsdJ+Lf2d14b0nH8sa6M1tAErKFxA+CN1x5eSJL4pzst9wdRR5Zl1dLb6rhd9InsuOr54GBAIlRhSGp07+AAhw8dJkWai9tUd46Ucb+HyiLKc0iJljZ6dBvniy1SylNVwiKS5fCsUdTYDsaYd+h5rg+L1J5UCBMptlstGe8nxc5MnKRkU1KgrXabWVb6jnMJLZM6tU1Ruly91ISr65mrlCneyxXtlyetSnFTKGVYQsxNhbRqti/FWDkcq+AarMZZi4iyouPN6fnDKEFsewhMD4Oswt0PPYRXNzfQXlhCnCfw2j3ZTqfl0KGaEtLOaP8Lzu+qauZEbTaLAEPKod8KlzP5citXGIZSlMaDI1IeFmHStmA3ldOqFOXPCw3Hds2yMu7Li+SXDg4u79A2nlVKu+Vq3LjQRVgaN4P6ta5uvPZDFZIfd3z49HdeQruG4chUoor7VUuFgo0niCBYvaVpjNFwKM/nUDGbXHS7zRg+qW5myiFyaHU7MEjB5UQ2CRFkSLdFNilZes6IGG87DrA3nWIcxURoJdIixySYijos62rWDsTlwbYMaDDYfIP2K5vGKMKEFgBVM9SArSRZBXNrVJ0TeaYSzuU94aEN3IbE4WvDpFsUqc+a7qf7mJJNIkGTlLUxmaAaByhp21O67ByMMKVjv5bSNtYO4/gD78c3Xn0NndVVcdEKaJ+5l1hGLTK517UsRvg1OdwucxTrxueaJyXxeZFWYDboJKXLZFtybpntMul08cJDFiJ0DLz/YTQVNcxV1Zwbz7IYtpnTuqP6aDDZ++Xh4NJZ/enV0ApYQ+PdS77G1x/+3x5IotF/btrlcS5s4lxpxvxUG9ISlJVESHEoii/himFTCUmEYYC274tq5X7fdpurgkNRlSVPIprNZxbvZZm3ayC3iHBcGyQmkRUc/i1kNjCrVZdunLKqxGwKCpttmA255kUFi/aFyYhFXxFncBx6ZEbEZhIJ284sJ9zkjEs6AJsUfEYk5vCwA1LRHPpFXch4wSROxLhDiqtoGznP7PXaCOkFxhx2pvsCy0VM5H/LfQ9gJy3AdcpGq42Exw5OAyysLEu/8triHIoskqEOXOWccnKXSZWOn1u2YtbdHOqWyUu5mG8UZnNOZGc5NM6jD8tKCt2iaCp5YZdWQ5xHttk6k/uWm5NCSr5st337M4bKDqbjy7/Z6Z/c1p9kDa2ANTTeZdi8+sihvd2rP+9Y9f2ebQhBmU0PqhQKMUHkpOY4n8pqznO9pte2GeUjZMFqlZ/DBMch4LohdsyCvKQ+LQnDKp46RJfSMpE7JiJilCldNtMQF4jIrpACPSBCjioiv4hUbMGGGwZKNtDgXl5S10zyHEKeEAFOeFFQJkTmqahetrvk1QNnTx1F6j0msgqIDCNSuER6ZUiPizJYOT2Gbw4SWkQkpLoVMtPHLi0I3kwybLsOrrgW/Pvvgnn+NF4Px4iJzLd3dnDm5AlsXL6CxfkFTMcT9FpdjAZjLC+tSI6Xw+S87uALF1KJ/zT3E0uOumndytk9TFY4kPPFjlu8qOFpSePJEDvXtiQczYMihLDLDEkaiwEK58l5YdH2rbaB4sejaPLXwmi7pz/JGpqANTTeRSjqfffypVc+3fKqT7Y8g4uaGzNHzqcqQ9pr2BSiYIKriLG4PWfm2sTEwQMIeOAAFzXxhCA2leCpf+PJiJ5LupPUHLfZWIZDJMOtOc2YQCbgglRw7llIWiamdLkQDvG1Cy/j0miC3bREarMapftLEwmRcsqFStJLW0konB2w4oTUZh4TUfM+pEijgHaxaVWqkgJ1xO1DNRIi3YLU/CBKsDuOsD+ixyuflHgbe1mNLVKfF6YRRu0upvOLSFYOwTlzFo9euYpyYRGv725jbnkR4XSMwe4WVud7CAb7MGvaFzr+pYV5eo1YFiisaqXSmRYBnAfm8LLkfmfql0+waZty7phQOQfMoWlTHLEUrl3bpEXBRPylg3Ak5z+h40ySkKc1yMxhfh9Q8aCLbNk2qp/Nw8kPTHau2PoTraEJWEPjXYC6zqynn3j4oSQe/TXPrpdrUpIcMlUyLLepwOXK3JxIgtUth5yZeFNSiFJARAo5ZwOJIiMSJmJkcnaIsAvOXYZCOky4MuaAq5PZGYoj0pwEZatHJnIiYXCrUa8De3UZe6T4/viZZ/D01U1s5zVGRNwHRKAjUsApEXeQJhhOp40FJhFxMBwjGoxQ0G0m7YfNvbxcRMUHWNbSzhMSuW4EU+zSy7vHT2CfCPKNIMQbkxC7lodLtJ1rLR/F0cN4eUQLh0NH8eSrb6Ci+0bDKYKDCZZ7C3jqiW/h3LkTuHzpVXRaNqajfdjcMkTE6Lms8EvJ+fJoQsxankTxcth6lvctmxIwaVNisubIAeeHuWBMwupEqvvDPVG8TMIFkW+esed2JKpezL84PUDquaLzbBml8hzcQvr9Z2Cm9+pPtcaNBp0D1rgh8cbFrx7f31v/JRvpfbakImspcGLCYOK8HjLldtOCFWiay/AALs5i44haBt2TEib1m3C+1XXEjzllwwzOdkoPriODDLjAyOI8Z1VLmJbbc3gbEppl72PXQ2d+Hqu3nMYz33wGT65v4PWdfZw7chQnFhdwuNeFx88h0uGwrpE3s3pZ7SYjIkCip1qZMoRBVWo2iYgWC6TAr3DldruNV7a30SMyXj11GnuvXkCn28WY1Oe5Bx/Eky+9hDUi4cshkftogEPHTuLCixfwwbvvxze/+Q0cPrSCN157GV1S+XEyxnSyh7mui2C8i2PHjohjlTIaYq2bUmpx9+LfmWCZaOvZuEODyRezPHVZSOEWV0+7bU8U7pBe35ReZk9UcRgq+H4HHVLnrIRZUXOovyzpnHMVuMEDI6qH6Hz/3GR4ab03f8uO/nRraAWsofEOxXh8ubt++bWftI38475rKg5rcrETG1mgtqT4inOYmbhT2TwXgLOqsCxnlut1GvMLQAqOWAVz/JrNJI4SeS0vr0mome/nMCxmgw1MUrTcCiQX6ZqF9N3apkLL99BbmMPSiWMY0e9Rt4V1UrRffuVVfPXCJbw2CnCNlPk1Vqxpie0gwUGQ42CQIA5JDY9i+pnSgoCULy0cciZfUr6btHjYpV1IO3N4dX+IAzqWsduCd+wUpn4Xj1+6CnvtCF67toN7PvR+rO/t4NY7b4XrWdjZ2cDRw0u03znuuesc5uZ8PPjgfbjjtltwy6kjOHpkhYg1JxItJGdeScg5b6qd2aWL9l8ImPZZrDGrSi6MximrbsLJpKQtIlMePsHRA87zRrQYiDnHTaTLhVzj8RAVE3bVWHGKpmazELYGRdFCnX+qzONPF/nI1Z9wjRsFehqSxg2FcvSG8+Qzf/r9ZR7+smuVhy2rEl9mVrcVly6TMuWeVCYQdopi72f+o88qWFpsOAxNpJsT6XKxVZ5nMtWIrRiztMCh1UNYWlyWdiXOdUqRb90YYTTFWUrCqOzwBJm8O9svUsY8OjDOKlze3IHf62AUpRhHCZ5/9RKeIkW6ORhgRNutHRcZLRSCuEJWsvJtY0IqPKHtTkipj2k/dqYhdsIIh8/diu1JiNWTp5Gycnc8dJeW8AwR+4lz5/Dkcy/gxKlbsL59Fd1+F/25eXzz8W9ifmUOi0t9nDp9jE5JSYq5LXnuVsuDbSn0u6RKaR9ZtUobkYTca8npsgtWKm5WmSh2rgZncmZF3JwWEJiFAAAgAElEQVSFJiTNfdZyDkjpzs11RTVfevOSFLpxZTmff56v7NM+J3khCx+u/DbpNjG8VKY4hokfN1SHDr9X5PmL/+g3f2tDf9I1bgToELTGDYXHnn3kfJ5Mf9Gzq7NsfGHzjF0eQF8oUXLijWxBDDe8lo2AlCaHlG0i2CiKxB3q+jAEruhlsim4ncgwZSTh+sYOkVSfiMIQ5pXhCNyCxFMCZ4YZfJ3JnmuyaqMp+vJ8IpLCwImja7h86QpiIt757hKs1gJef2Mb8ZRI9fVNrO7tYW17AFe5SIMUx9fW4O/u4cSxJazQ8QThNdpWB2Mi39XTJ5GSsr73wYe4WRlLJ0/h4Ue+gYc+8mGsD/aREQk+9NCDePjhrxAJH8Yf/eHncfrsWVLwi/jQQ++D71qIghHM1UU6Hgej0QTPPfMcekTGx44eI7V/XELDhVchIeKt2FCESFGUbjFbvzNJ/ntmGezYxSF4iFpuWrTYq5qnLqVJLqkAXtzwLOG5bl/aswajAfq9JamWbjyzHQlDi+c0K2KpWueuZrw3zZO/kky2L3q9Q/v6066hFbCGxjsAdf2j+MwP/0YnDg/+U8ssftRA7nDxEGYzehu7xKa4ikcCscuhmvXn5mJarCQcyqqM1R6bcUQxh0xj6V2d78+T2qvw+oWLCIMIx44cllYaUb8y39YU4i2VZJlR1E1vLEtgLuDiHDKnR7tEniap23gSY7g/Jt7s4M31LRADIiICmiYF9schLm2NsU2vc4HIOPdclG0iZJuOisg+YkMPYvZzD7wHz7z2Mu557wOYW1qGTarSIdX6rWeewN33vwfPPv8MVlZWsNDtIhgP5ct+6vhx/PhnPwuTQ8lZjB5t2yWy67Q7WDt0BAsLSxgOh9jZuYbRcCzTnfj4TYt7lUs6R6mEm9lAg/PdObcSsamIDHyo0CSvq7cGTXCxFrd79fs9jCZjbG5uigJ2aHvddlsWIq7rY663AJ/2X3FKgNl25gB2vdO6khyzadP/D+VVfeWv/xe/8vI//cf/WFtVaryroXPAGjcEDob/wNzdvvy+qo5/WKm0xXqJCUJJj64hxGuJOYQhLUjlbDD99Rm7lqngclGV0dzGilcIJi2EeFnN8deFHxOGU1GETOrS21Q3M4LY/5kLi8Rly2wKupiYOaTK038cWhDUilTtyVWwFWYcR0IuRUWEz0ViPIOYHssWGsrzUZkeEW2Np1+9jG++9Aa24wpjy8M12p9tWiw8+9orOH/3negvLaDV6+HNjXVc3dqCQyT/0qsX8KGPfgyvv/kmVo4cwta1LZw8eQKf/MTHkUehFI05XNBFhM9DFixli0JdXT2EBz/wIbzvvR+gY7Gxs7WDzfUNRNMQHpGmDJbgXC1PUeIe4H+v6IqrnWvxoy6bRQi3bXG432jIlKvO2WGMw9WeR8eZxFLxLLOQ2fNaKcnByzaak9r8keJzU0sxFldmH6Wd/pGldn5Sf+o1NAFraLwDcOnCU6vKyH+MyPe8UrmQHTssMYmwfSSPVJCfqpmlyx99ztByEZbtuGIwwSTAhMokLDYbMvuWSKqoSLX5TW6XHsM9rMpSGE8DIRUmdlZ9jk1kzoMbuMdYWfLaRm3NirwMKWDyPAuub+D0ueOozRJ7+7ui0h3blu3XPPDeclGLqm7y1bZr4+LVXXz18ecxIOVZk8r1V5ZwZXMbwSTCE08+hc/968+JmUU4meD+e+/FYG8fL7/4kmzz8Se+hU99+lP4vo9/jBYebHwxs7FsBh7TrnOotzHYSKKU1KmLpcUVPPj+B3H+7HlutMLB7q4sGPq9LiqOEojKLYV8pWSKQ/FoKsKlwpzUf9HEqeUcc7tW4wsdyXvAeWVewbhuU1PV5JAbD+nrU5YaVQ3ZrpAy9xnXOWyzfrAq408WpS7I0tAErKHxXQX90VaT4bWHjCr580adWa5rNgVBHAJWTVEQqytRulZDsE2rkQhQuLZDRODJbaKKZxOQlBBFE+VkRdsUI5mYTKZCbDLpJ0/FQKNJg9ZvbZ+3wSYdUkBUcS7aJUJu/JTZ7OPo0UM4fvwQNjfXZTsVWzXWtE+svC22tCQNT/SSOxUyul7RwuDSxg6++si3sD8cIWbXKFocvP76m9jZ2cVwPMR4sI8qj/CVP/kiPnj/3RhuXsEta0uIh/uY77TR9l1R+ryv3FpVmewrXTcOXnSj53iyzzJWkA1Byhprhw7j1lvP49DaoSYikBOpSsIbzf6KOZcxiwI0/cvNIAZDZv0yxCITPBWplHw6EzITPh+y77fhsNqfOWhJ6xYvCmp+viPPy99qc2rC+1DlYlXln46m+7foT7+GJmANje8SsuJ19fyTv3e4zKc/YqniqGvTH/Ysa8whxLoRQorOLLzM7MMOVpx3NFRTRMRK2OHfeeBB3ZA1j/zjymUuFuLnlU0MWnpb4zgRp6y5+Z68zmg0EFKrZNAAZkVJDYGzimYi4QIjGVbPBV+0P725Du55z12wHFsGPTTDHXiogi19s7zvlVmgNImAa/aJduW+y5eu4ZmnXsR0EpJaLfDCixfw6msXpYL4G498DV2fCHQ6wIUXnsLGxZfwpc/93/iRH/wkVvtdZGHY9Ony9mgfuHo5Z8MMITdLCsd4IcDnQ7qJKiX90VwBfuTIMayurGGhv0gqeF7MRyQ0n3OYPkdNxFqkhSw2eERhzh7WYv/cuIvx9gLaZ7mdPbjp/HL/L+eWm4hD3YxuUs3CybKNt4w+1Mz0oxKDFCLqIjUcx7g/iYNPxtMrnv4WaGgC1tD4LmCwfcUe7K9/2LfwEc+CyokwbZPJoZQcLYebOd9bzib2MBFWNWZez4aQLs/EZevExsjKlOfx/RwuZTKRoQhyW1OlG0XxW/nLdtsXwtja3hBbRclp1rNxgLQtDoM7FvcHN2qY1SB7RoVxgJWVeXzww+/FJAhndo1ETLQTLitm2me2grRpO1ywpEr6qha0kICHKxeJhB9/hY7HJvXYwfrmDr712ONYWl7EI1//M3hmjTQY4dYTJ/D9H/0I1pbmUSQhXWifZzlYrtDOOJfLyrQqZ1ECYzb/2JCFSS59vRCbzGCawHPb6HbncOzYSdx55704feoM5ucXhIzForJs1O11P+iqun6elZh1cLheiQWoI7lh/lnKTOIMURJgHAwwGO/S7yE4jVDXqVSu82KEFz48gUnsLnm5Upd9yzR+KEmTc/pboKEJWEPju4DJdLpWFNGnbQvLNpFG2yVVxfOB7BYHhKWXtJqpMinGMpqwKPst8x9/VqDc5sIXg4iOw7qlDByoxcuYwWpRnCbp9owIPExSIWBH+oAr9HptGSCwvbkptpVNJLvJ34otJb0uV/6ywuTZvzw/2HZMopEcd9x1GkeOzgvB1Ow5zSqPpySx8pvlU6WMjNSordh5y6HXdHD58jU8/tiLRIiLiGibXruNnWvbpFKX0Wu3cOupkziytox777qdlHIowx5MxSo1ld5mrl6GtAw1IWRRlzzXt5SVRkOc4mY1m/mrGlXMirgpRmvhllOn8cD978cH3vcgVpbWxHMjo/u5p5qjC1yQ1oT1IVXl3DPMtzlOkwrgFi2TPbaJZA9GB3j1wkt48eVn8dLLT2P/YJPeh5heO0WaR3TeE9lvziNfr1w3lXG3qsoP1tFI+0RraALW0PhOgkjC3N3e+l7isz/nebbRlANx6xEbOViScxV3K3G2skSpSTFUQSQ6DUl5BhiORqKOmYxYCTfqWAlRsjDkfCjnh7n3lcmHVTQPZ2CzDhkoL+YTkClGLk8ZuvKmjPJzPVtUsEO3sQuUVPhKb6sh4WsOy5IuRL/fwoc+dB+pdlJ1JRG+UTbNgZyr5fapWY+ta7M6bcwteCFQEsFdWV/HVx7+Klp+S16T92k0GqPRsArnzpyVRQBvg8mLGV3StRIJKP9dyJlD0BVPNCLl7VizWcmGWEY2ofVayJPV//U8MP/McjoXRY3FhSV88IMfxm3n75DIQxKnzcCGSjXnNc/ldRNSwEy+fJ3HQLq+R8o3wXMvvoBvPvZNbGxdJeLdwe7+FgaDbcTJBHkRifJl/2hW7twmxvvBBFyVeZ/OxCeycnJYfxs0NAFraHyHEOxeUs8//cWTeZl8ttVxVohbYRNRGqZPn2pHQqiVUk1ulYuFaiWkERE5jCeB9KTuDw6wv7/fDBFgdUh/3NnhqRRbxebrkWfXi46U2CsyeXKh0GQczIhKEfm2JffbJSXc6ZAS3dmi7e5JPrMoGsXL4/h43i+7YfG+MZGzCq6qDOdvO4VzZ4/R62RSGV2yBzXvMyneuqTnkUK0SiIvUoRGEcAxiAA5/O2aOHVimciObSrHCMYjjAZDvPDcC0jjBO1OFwlPVSLy4mAzO1ExEWe0uKjpXFgcJufUc9WYj7BKFccr1ShgmRhlWdIa1FQiN4/jJUFZ5ELgXLTFYpqV7333PYATJ07O1HIzjMGxm9w7n3sZzkC3m5Yrytp2PKxvbOHpp5+lhctVOb9+y5eeYT6vvK/8nojntITDC7nwdKgsm9JrTs08D+/N8+mDXIinvxUamoA1NL4DiPI9bzLe+khdJR9QrB6ZQLl1RTpyjWYwgBTr1qhmt3EYmsknCiNMp6GoqIz+yAfTMabxVBycOOzcTEPiwQtpo6l5vKAQQd3YVcqc4FhIgUOtURyRsnOFqDw2tqDL3t4edvd2JedbMLFi1rLDrlBmE9LOuSWoStFqWXjwofcQ+TS9srwA4PYllRnwaxMOLQr6RoqTczbuP7OMe04uYZmU5FrPw53njpPep0UBEz0fa5ri2KEjuOOOO8A9PGwCwsfF4V6ufOYQrhAhz/YVm+ZKrouZRjmb7ysDK+qmAMoyZ1XdhpyL69XkkLYuQ3LcTMTcvsQFWWfPnJFFSjmz9WTiFUexgiMIBSaTmJ7DdpOe/H71ygauXF4n5R5Ir3W325c8M6cQuGArzyp533ifOVfM5FtVPJ6RlHE+pt/D1ayIHyyy7a7+Vmi826CtKDXelRjs7h2t6/xTrZa9UIvpYpNzrcsmnyuhU+SiXPljziTBf8gxIxEmDxRNgZAlLk8FkWpjS8kKjn9Pk1T6VFkF8u+MaqYCuTqaVRz3BbNCVg0nSSha1rV1hmkwwS5ta35+TrynpbVJ9tOWBUNF5G8ZjX3lsaOruP32M3jqyddglBJAh0Uq3KbXOzK3hJ/7y38etxPZeq02HU4LT7/4Ki7vriMsI7i0AJnvt7G9tYme38LHP/596JAqZ8JyeZqQ0RRSyYKBJK/D0YCaTUmU5KhJm6JgMnWVmGdcnwoFUbpFc74sruG23urVZXcrBp+Hpu3IlMVLu92VUD2HjLk4zZNQcQXLs+D7vhAxh6m5onsaJFhf38HefkDvgYuFhRUhX9dtyWALMVHhUY9STFeJe1lVzyqleeFQ8v7DUbnzvul49wztztP6m6GhCVhD420E/cG3v/pv/4fvq5F+j+nSn3LDE8MGo3zLuF/ynny9Md2oRQFWXDNlmzK1Ns+TmZVkPRtN2DhfsdOV9O3OHLLSLBHTDW6LYRMLHirP2+UirIKUaWWx6m0J2dh2Q06tFkQNc640CPZhmQVanR4M22t2ojJFYXu0ffafZqlOB4GHPvJ+XFrfxWCHFLWRwCLVffuRVfyNn/wM7j93FFU2oW0RibYNnPzkRzAMR/jDr/wJptFIcrquY+InfvxH4LgmLl15AwtLi+j2enJcRpkiqfJmeERVyhze2iokhMzmI9JmxXlhIlyuIr++MGDa5ZGLYtjBYfjrbldAo+yrTLYvBWeElAjZ9zz0+z76c30ssUUmqdmF/jJ+9C/9GK5uXEUYTJHEES6/uYmdzSGCIXDmxDIOLR9Hv9uV47juL329L5jf4EaJW0LuxmyYRlHGpsq9c/QpuKuu0+eUckv9DdHQBKyh8TbhhWf+4CRU/hc7bW/edw0pHirSsrF/5N5R/tst4/NYxZUS/p1ZOMh9ueQRcwnNXjfdaKp/IURkzsiXCboxlLDeGrXHirfba8lYvWAaCslwyJWLra47e7CC5rA1F18RTdBjA+S03VZ7Ho7dlnA4CzkOZ7OLFldkQxWYW+ji/R+4D1/8/S+DxfuptSX89Gc/iTuOryHc2SQOiuB1O6jTBC6RaLfn4747bsPm4BrWX9/EJz/xfbiLfk/CEEkei8nHarqCxf4iCu4tZkOPWS64klOkmjakNEZ3bkHC9xyWNxxuiYIUUZkzMwxeoFyPINT8vKqQ3CyHrZvWINquYZMC7uCO22+n82E0RiWkvKeTIb0/XC3ex/lbb5XUwMFghPEoFHVLL48zp28l0p5Hy7dhORy+sCQkzbM0rltUWta/+3MlnKwaBy/DVF3X8z4wmmz9G7p1qL8hGpqANTTeBkThVfuxb3zhLziO+X6edMQVyfwXnaftsHKFWTUOVlUhRCOez5KvbByqyqyUvC/nFCGe/44Qa87tQabZmFLM8pzSs2o1vs5N8VVjV+n7rhD1aDLC/MK87Fc1I26u+JVeWrsJU3PYlRUjFylNRyPM9SwimTaiIoVkhmnxkHNBluGIocVdZ4/h0ok1BBsH+OT3vA+3nzyCeLIH3yIC9ztISxlkDMQp4NmkGldoLVLgntvvxP1338POJHSMGRZI+XJumlujiihBd2EBLh1LTecgN5qRiwkRuecoUeNxOJXiKGFeCRU0pMf5ajmvHDw3m1YsLg+vJY7QFEbJWEdeYLTYzETh0NpxTEmdj4l4uRo6jjLJ5TbvSyajCHk84Z/73g/jltNn8aUv/JmQON/f6y3SNml7BbcxuZJDN2f7wITPe8Kh88az25aFjGO7Nr0357I4mdcErKEJWEPjbQBXuj79+O/fTmrxP/E8Z64pr5JWXPkpAxdsS6ihsT0smsHwqpkFzETLpMM9sKziuEKZC6b4jzkTJ5Os5zXGSlEcys+mFUe9pX5t2n4cN25W02Aqt5P6kuEEUqjEYpY9pdnIomBlWMtUpFbbRxlVmA4GQLeA6ZlSwFUTUbtMbHEtfcBzbo2PvvcMeveew2c+9hBavFAgJe85pAAdQ9qqspReJCOiJ1LicO3xI0dx9OwZrC0uIk9iVFkiaWg+Jzz0IGTP6iiG3++gO9+HR2q9lDB0LsRnENm5pHqD0VB6oc1+v3HwsqUzGLgeDjZUc67rWa+wLHJSCc1zyxaH3HteF8tLa1hZWcWUXpcrmZMkxc72Dka0/aL0ESdTCfUfPkwLiCOL+Jt/6xewsbFJlw2kSSlKuK6a3K8Ucc0iCtIrXddv9XaLixa/5+zqlRdLrtNao928pL8pGpqANTS+zZhOXuseHGx8lgTU/RxabtpjRAA3Ck1JOLLpv6V/XOjEypNDqexQxeouIeXHlcas2hpCtRvf46xRwNeLjEyZQWsK0VSz0Xqcj+THSy8rPTcmMg9JZTb+zw1JXw+T8nW2vCwl3N302LY6LWTTDBNShk7dht3zSfUmknd1iOo6XNgUJvjI3edwor+MLnJYRG42kagipVrRa5ikqMGExxXGRG4lHevt529FZ3kJQyJ34kxSyj6SPGmmMynIooEVZDyZSDi+tzgvIwgbP2c6R1xIReeg2+lJMRvniLn4SvLqzOSs6I2mYYLPgbQizTy2+dzLRCRWqKSCuRKaW6dUZaDj96FagLviou218dprL4kpiAEP04TUObcd+T1YpKqPHV7D4tw8gmCCLC5osWHKIqDb9SXfXskxN4VkPOSCw+F8XFyFLSFyw5inxdPpjYPdx44urug8sIYmYA2NbyeeeuzhD5Bg/YxjVm5TpFM3BVeQatgmNzkjCKnaJRVlmLW02RisjLmPl40cSPWleYqW1Qxg4FysFCqphpBlSFAaC6lCzUKf3J4jHtGWqGI21eCRhNPpFC3PJ0VnNz193HvMFbushqum2pr7f7m/mDmr1W7B6/jYH49oDZGg2+5KAZdJhOflNVY6HSwTgXZ5QlKRioOV4ulIHIblY+DpTW1HKq9jOo44rdDpdkQd5tKmk9P+eLIQafLLhczp5UPh4wtGYxke0ZtfIHXM++2RQo6lKIrPQfv/ae/Leiw7r+v2+c5051tjV/XEblLiFFJkKMmSCDux7Dw4NjwgHqA85E/kNUCQx7wGCQwDEWwnThzEQ6JERhzKgWQokq3BokxRkjlLaonqsbprutOZs9fa362mH/JC2bIa+D6i2N117z33nNNdd31r77XXGisgCqwzLbShY9xgfN8di6NU3p8Z9pn+7wFMd7mccxyrlw+ohMZ71ijJx61sb+/IpUsPyfUb3yNTTvTvp8Dmg733iIYn49GWPu+c3LlzIF1Rch64KOd6f8za86x1kNqYGEvRsaVV6ZrqRubRvKwgzw4AHNYDscIccFgPxHr5Lz+5VRWLX+6l8kRPGWjq82NZ9vVxd961nyXoxvct10w2ji2JCOCzXC34YR4xIzgl++sosjJWxxEbJvVZ/i8WP/iZduQIRHCMgrBqgbldP/NqozJ2Tuv3XQdAmPVipwR3TiEURpNc1cryQFnrvJRNZZmXJ1O50B/KpHOSNegLRyyRtwwiiCxFSJ/X6DFXuiEo4AyFTiyEZY35XEOxDNCuV4V0GCGCD7YCsyDFSMEwhejpZC73rt+Sk9sHMjs4VJbdyZYCb67nWC5XUi4W0hQG/mDnOI7l/N63qMQ4VRqnHBPCtUEdvlii1D3jfUJ5HcNUCF2oq46/R6DD5ua2gm3rRV2t/Z0ocCNVCu0BHHl7e5clckQ1rlbwya71sVMonqXpSpa9syzmZmi99G+8V9bFQ3q8fvhpCSsAcFhh/Q2tw6PX4xs3rj0/Gvd+tt9DPbWmVshxnjezAACxEAUyNBf5kmnMmVbQIXzYY051Np8pAK8IwABlMCgIsMDCnO91UoDlrJTdHwz8B703ptDDwnISC71fzAdXvn/MkjhU2ARjI2EA4NSZihogCRBGf7lWoNvtDWXaOOnPCrmQ9mULwqdFIXHZ0I1y6YMTuBCNq2wX4FkpW8U9AKMv9H1b79xFqwxsJODehVnbBsxamTv7xcI/KzeWrd6Is8UXxtvSb2LJFCBjBcVM71WOsruy5tVyJoWCH8Abvd7Oq56tbG0pj8j1RaoUviA2A5NHfxfWkegfs5IQJX5GuFbQ7CsLvipbm7v699aTNOmbw1hrdQy8F5g8WgEXzp9n2+D719+W27dvynxxQpDGjYDwK2KloWGFIbZ4SdfV7R6CpsJPTFgPygol6LB+5Ne333p5fzBIfs256lKrrNElFk2XkAWb17F40//Og16CuV4m8li2LcrOAF0wVoiwnC+j8jGvcEY6D/q26yQjALb1iBtZOz8BVNZ9XjDlkm5aFYVNYMkA4Y7pS/cBO1aQGSjYrDpzoYrqTvoYq1lVshMPZHuUyJbS2B7JspVWGQoIq8rYrB8Bnp0ClMP1uFbqNJKZHmsGNr61QfBrFOQA7P0UCuKObDiOc8nGlsiEkjM8mKEIRx8VLNVNEoGfRavvNYdtJQxIlGG23KCkpoyO2AnmtRjMOwY7mF2lxS1KbsjMuMbVKROk6AK2zvKlEA796YlcufyoAus1KqKtzC/cyGQZ+ro+y1mff/XqVXnjzVfkzbfeVODepxALIRBWvkZM4oi9YAjTpMGNSqa9rL+hJ3It/NSEFRhwWGH9gOtrx2VUV4uPtNXqp9DWBdBgGYh4Z6bWPuAjxt61BFXTAPsUHtosmioa7HdVlObjHFk52YDScW4XH/IsMXtzCnN86s5MO/jeccTv43kA46Uy0jO/4sYk2ZEpw/iaiOw65u4gVrBATHEzX8m0i2VHAW6g7LOnoNzTY+QETzOegBCp9dGGkcmMlckqNCujLVH6ZkCCskaEH8yXynb1OMqd+wquE2XUu6NN2Ztuy95kUzYGIxkpKCoEk+1GRSMJApGUKSuN1t93MlGAdlBH67Fbb02J8aT5/JRiMQjV4AGN2CMooRnt2LRnc8+4JwDeiD7OBQVfLE+zVA2jE2HFot8fyrnd82TOxpLd2Zz12mEMwIrnPfro4zzG5z//Z4yHPDk9kdnshGXruZ5bZ7JssGf8f+SSODDgsAIDDiusv5F/oG99cvfOwc2fH4+SS60ysyjrUYCTgpkBRMUHtUc2u0ojDWWxKGt2USMG2C1Vzyf44Eb5WQEnn24oi0towEG/ZGfhAHDAYiiBvr4AW7bYO5araz9WhAW2jJQk9JFLiLr0sTRPCZg4B4CQkbuYvs8sJOt54Q9552RSNrKnoDVWVHJIM6j0/WqLRwTVhLjJLUqWr51rydpPi0IWmJfV96mZpaTMWtnnUI8HlTECFvoDZbyxd47Cr2CqYM8A8BrVg4yqcJbovQraWeSRnmQpgzyRo1kh5Woh2WDoBWg1Z4exeUggUoMbFfyjG3Pfxv1jvzvKbPxJNxZgqB3GpxiQARU2I544L43XTDcmejoKqCeNVSwS6+O3jVmnQPTmqpiRkh/96M/IwZ0j+cM/+KR86Mc+IA9dvqTnpxuQrZnezqle84aS335UVvNJM4un4acmrADAYYX1A67j268mL7382ffHrv0HeeLcqkQmrH6oO/tnm1CQFJFRAkxqzvp2FFW51LyO0VsEMJYNUpCOOZuKmV2YZVgqUUcWFqc+cIDlawUYRQL0V8GUKeBqbG4WkGrgmzPpB71QCrjwuDLrDKMxceznVy1BCUAUuZimHt2qUBaayN5oyH4sWO1w1NfXl+YUDQcvMMsaeFhxk9FKKZVi5LxYCnjeIOsr4A5kmOUyGU+lRh6w/h7vD+Zee/YK4ROYewxmH1kiEQRgNr6lv0c5X6/NUit046CbgD6sOGEycnwsPUQxDoeStxbqkJGx9qlsNnMO3dh0pRmcUIgWWR+ehhn12TgXStT0l45iP65Vc54YgiyMgpmfNPYmiC2s7ZrpSmYuZKXes4/903/G6/rUn/wf+cD7n5UrVy75qMKFTHiSHjoAACAASURBVJnJvB/pP4t+UayCCCusAMBhhfWDrhvX35wu56e/OBm4q40CFJhcXdTSJi37t6zUerVy5xXQNFtsrOSMXjF6s0gBAiDdu3eXoy/oHRNsIwMMSyhy5tSkLBHRgQBylEIxVoPnIJmnD89nujk1kvt+KkvD7M8q61Xyir4res8IhMCEDUAcQq6m1ePre7tVLVnZyrlzm+JOZ1LodYERc1zJRWeGIMwthqpaAag3zBljmE1GHEVKBj2yUJa0FXQBvDXYc2osveM8csxSMfvRlZV0UeqNuprA7ru6COY1i05R9polCoalzBcLs+pUYJyXpfT1fAbtWFl4x/I9QBXqcBqdRDb7jPtt89LOjys5zhGj/Nyl5uFMttzWZ6YasO+cwHSju8deuosb/bs60VMa6LX0Ca5gxjh+qez/l//JrzBM4isvfpkuXEWx4N9nA4/Nrifj6UDxOkrDT05YAYDDCusHWPgQ//yn//1jWdz8RKrUqasr9h0BsQA4qmWhVkYRGmXWSDj7i7GiorLQeNelJpRSkDs8PCT7BdQgOAAf7DYqE5mZA8ApTlmKxVwwVLs2X+r4BZUvStMjRBN620kyPGVrfbxOAbBeFJK0NnIER6k+RnuUQZ7MSrl9446M9XlZk8jVrW2ZKCvMNqbSjfvWY+VYVErG1629j1Njjh02B/qVxcb40fvF5qIrG87RDno5gRdzx0J2bw5WVGY7YbISIxnZP7aIxZZmWjUzlCMFdwAasoNvnx7p/dJNTGRzurGeQ3061+/By7ph9jHu4WJZk73iFpkXdGcMFkJpVhUcxWKA4ibvOBvsIvPahjjLScp7jL+H8WhTDo/u0iDFhG0zbkTAnFFZaGL7mILV5S/8wi+xqnHrznWZLWYK1lDA39TrHOhrNyTvD0IucFgBgMMK692D75tyfPjdtKqWP9nvxY/EScMPdkTY4gMZ86IIswc7de8wjAA7haAq0w94lHsxCrQsV+wnYpRlsZgTAACQWZpb5F4HwIxYjqbemIbHDY0hbO5VvNgqUwbcJ8sTPSb9nqGcjlO+T6TntrW9JZsbm8qEdWOg54DghIUCyVsvvyG9ZCg5weyuHlsBdjvjKFEySGSSTSVCyRqzu3qc/mgkbeXjA7PM8nw72wxUurmAX3WmoJbFJhCrlKkmecq4Pt2DUChVK+tNKPPG68CEObfFsSZYcCIBiSwZJW+9/tNqJdePjuRwtRR4Y4JxF/O5qcnBxKGwbk4I6tiMmBANG5eEzBjMFOzWwNiqCk3UkpljDhinj5nmhNGOlY12sTcdUXk+HKDEf1dWy4osmqXtKKFXN3vt7MM7ViF+9ud+Tr785T+Xk9N7uhGx0v69e/dkc/Nyu+FHxMIKKwBwWGG9i1WUVXTtO197KE3an8qyaBh5UKXLFBJ70OP0xhDe58KYondqsj6jsUi87tbBLTm4e5clZSqc/agRrSPBOKFS1l8bhse3/DNL2OhFNmtVtbAXHEUN1dcJe7UN8XqqTPfJRx+T8WBE4HYKAksFqsWqVFZZE4AGSDFCwPx8BZIqM7mHwV6JRynZrcMcb2kzsOwD0yDK0U4TTB1Cswa9V73+HuePKyn0biA6EP1rZiQgOhGzzRCVoSQOgZo/d2wwHNglbCbpT60vUOAtpZL5aSG3Fws5KkppENeIUSl9rD8cUFUuVJebIQfuGbyfDUhLfSjz96UlAxZpfe83Ohszwn/LQhmyK6ykjLK0ry5gY9DWEMCNZTSOyYTNoKOQosa8c8EeMfY9RTHgfPFo1JN/+JMflaOjO3J6eko2PRydAzuvyqJehp+gsAIAhxXWu1zfees1d3J08L6mLp9tYW4MyFCW0x/0acm4gAIWXsgwYhDrP4KRdt6fmCyVLMzGW27cuMEP8WhdnARYeVOJzkcIig8dsJJ2Q6bHsq435PATSATdCCxyZSXwvXPn5D1XHpa+slum9XEeOWYm7woe0wou++cuyCAZiauPTXWtxy4WMxnoc1plvekAw0PWk4199CFnbxXEarBtMsCEpWdYYCLtqO9NQIqyIAOMGkffZ0EyUWm5vxFGniLhpqBxKNPr+YAU4xj6VFpp6rngC9ETpd7rGo9gQ4KeMtgleuB2d7jpAcNdrRqakYCRNrqJiKKSGxeotVlVwIaosU0S7iUYd9UsFYSd9Po9KtjzfEiGS5tLsbJ9lg1kMu5YkudmCnaben+WyxXZ9sHBnO95fBz794+U9e5RPR1FfX2JOzw8Oj0IP0FhBQAOK6x3uXLXHy0Xs+d7WbRtgArwUIhSdhbpB/EAEYHOovLK2jya3RpZRbywyhE8j09O5dbN2wRWsMiKzlEtZ3MhYUZJFs+HsArMs1QWCFDr/PwterNYYHL4Hud/25ppSFcuX5Lze/sciyq8CUeS5yzZwjIS5em6mMnJvRM5Wh3Lla2R9HQToUhK/+ZREhHwUGaFSKmNGgIXQySaFcvWzCjWY+fDHlkoyrPolaI0DHBFvxfn0hQVrx7iL7DkAnaUYJL6vQrXmSi4KmAtWwidain1ojHS1IBlY7SIt67199EZ6PpJpchba7q2tWzlrj27jynbAIkXtVlmsFUiWm/fWbFKURQRsbasevpn/TscFARibDgS/TVSJg0QHo1GNONoqiVZfq9XMjqx8eNL6PfPZnNRpKUITuSYveI0GdXbO8Nv7mw//Fb4CQorAHBYYb3LdevOwZ6C5fN5lulncif9vM9eH4IW0K9FSRZiV3zQY7QGoh6CtCGvlW8hRtL/rl+/IScnM4IswLVlfxdZuEtGBKbo5Samom6Yc9v58nSzxnOCSQbwl4yhBWBfD128LBMFC/HOWJZPawDUtDaqVEFRrWC2vbElq9OCGwAA42h7k+wS0YFZL2WCEAReSaog21Z6fnpuGxOCf+dsdhbJRxAcAfgw5sRJX+Bc3dLtCzwS40NQY7eMiNJryhLp9LmYIV7A5EKvaaZseAWGrcDZpLn3zW547hmAvEWP2ZuIQO7kfbIzPZ71fL0pSWuK5ra0dKJMWXriwErNFtPGkoTPhQMZx7H0sLACBejO5icMgkA5HeYdmV47StNQjGe53s9sTACva71v0lDJ3nUDXv9wOKKArPb3N2EvvH+trLtP9MeXboSfoLACAIcV1rtYyPz933/0b5/NM/dYD6XZTsFjVbDcfHB4TxlmJnv75yWLO/YWwcYQV2fzuvfjA9fhCJj9BbOtlqXv6ybMp53N5zIeT6iAZr6sH6PB6/C8iv1iE3dR8awgVqxW0u/lsr+/L6P+gPOw0u8TmFBuRQZxBUEW3gNpQElKED45PdXz7clgPJBGGTAi9+bKlEcKLHXZSYpeqwJ3OV+QJeZ67HKO0nPGc8goWLJRoowmIy39mlF2RzJTx56pAWWMfGNsIvR1Cz1nzA7PFawKqKdTZfhAQQVeCMWQWZz4Wa7I23jCLKSCzgx9YJibJGYTCRBl0SAy32bbdKR8DdOkMDrVOSq309Sxlw2NFwG9NovKprPUJHg8w8UKL0ZJ2oB4oLdyoBuUIYVWk+HEet3UdHW2odFjDAYxgRc9fgRURC7Gu95oJfntjZ0rfxxFURt+isIKABxWWO9uRYNh/nC/324sizn7nejfIs7v1p07cufOLY7OINru6kMP0RADTk4AoRJeyd4YY51CBN5KO8qqIXOu6N7kZFEqKFWdvt5K0WBSsFqsWkvqAaAlHHGJyJgBeEhgGgEwfG8WCl6IuOirrN8DEWuY7FMpWKUSIwdYH6v0sbtHB7I80be6cyAXtzbMfQpjVSlY8ZJiJIAXhVMsqydk9QlZZ8TSMi0twWxh/yje7tLnFDf6XhUAXRnjsQL5wcmJLCCGAsNUpg4VNwA7ofgq9hGOkY1joYes36fYyVtDmikJRG+NHkOvQe9H2ZZmWsKRJrFjIfVJP0Yc++et3ueVd8Uy1TLntaOePtlU0riHAHMAcaXXOT+eyWk7M5UzUq50YzDW892cbslotKG/n+qvEwqzENnIcbGo1r8n3PsMe4uvt1H6XyY72787ya8ehx+fsAIAhxXWu1xNt4qHg+FOkqzynn74ovQ6lQ36AIMRX7x4Xm7cuClvvPaafOPll+Xxxx+X/b090ddYKRoiKRHaTU4mY2W5Y6vVeoaIGWI4K2FUZ4E5UkQSgklCCORsNhbHAOOEHSVmj51XYKW5BRpw9Af5vcrU0IdEaTlmQENkj8eVrJY1AyBQ2ob95aOPPSoDBebTQjcTGDfSY5Fxtxi5QZJSjxsAgmKe6hfcuDrOM6f6GPqypo629+p8ihDL8JgLFiu9V8uC5wGHrKmCb67XH+nzazBpeD2jzIzqgLN+MUrPLWejY9/zbuRkVeoGBV/wf1YmnCUEb+fncWE2EnFe2hzA6FVd6wZkEPt2QMu5YunkrKrA10W2yUgYcJGTyeKrwmYIo1N6T1HNADu+d3iszF9ZcW/COeHRaNqOx9MyzeJZnOTX9ah/pcz6i1GUfnaytfvqNL+8Cj89YQUADiusH2B959o3elVV7Q6HOcuNiRuxz4tZ0Ek7YUn4kYffQ7B544035IUXXuBc6vPPPy+bm5scMQJjHej37ty7J5cuX5K5fqh/4YtfUDCpaMmIMRvMuM6zXDIXeeMKn+Gr51AsF/w+5miLxZznBTYXR8auIYpCSRTlU7BklMfXc7ZgjTiHNgOjrDkmtFos5PVXX5UdBd0+jCWoqlZ2qdey0MfQ50UZGvQdmweopNGNjtPcFMVevl3xfXMKq2qy4M4eBzPW8xgMBiamQmk4NQB3ML0AAKNsHVufGmDemlEVmTC9s72fNrKWB72hgvBKThYWy1jrc2uYneAeIA0SwIt8YfS9GWYha+WbrKXm0Tt+PWulO+++JZ1n2RBx6YdQ1tLukhunytKrGtwfZbkns9Xy3r2b383z07/a3a2/vL2788rG5ujtLBt8d2/rkYMo8tL3sMIKABxWWD/YKpYnQ2W70yxPWebMPLMEQ0uc9zaOLWP2gx/8oBwfH8knPvE/5ebN2/rn52Rvf59AZKXmTorDlTz95JNksn/62c8qs7qnbGpMZt0owyuWKW0VWeptLLTh6qWL8oHnnlEWOZSDuwdMAgKLnGyMZWtrk0DRU4BPez0CG8qurrbZWMcwh5bMGES6WDV6nJFMRhPJ9TnVbCYn8xNppxPOrzZlSzAqlOHnzsIMsl7OGeCmMztMbg4gFsODseUedz4PGepslxjIQkwWkR2bCAr3Ca9tldWid0ykhEIrtg0ArakAon6Ui91T2ETCp7k/kJFuCtp7IsfFUu9RQgCmyC0yhTh2Qfyv8zaXseMGxYRwNuNlQGye2Cbcarz/tj2OTUhJlXNLtbWxZ5SZs1XVRC8r+39he2PzU+f2r776xDM/cRR6vGEFAA4rrL+lBf4YI3hBGdhiNqc1JPqU6PUm7BECgAoqYVerhXz4wx+Wo6Nj+dSnPiWf+9zn5KGrV2Rnd5dseG9/T7YnGwqyK3n2fc/I3u45+cpXX7RQhtMTWcLpCUECAONK30PB69KFC7K/v8vxmo/92q/K0fGh5QJjA5BYuhCOB7AAI3Vw6cLsqxjolRy9iQgoKEfDIOTu7TtyRzcIFxW89zc3FIhrmS0LyTCXW1QygENWY4lHcJQCUWyRC6wbCfSYUZoGwHLsBsyRVpdMn7AYv8gYOmVpHJcy5ylnA89MVrKYRANsPLaOSewYGiGcL6adZNnwOTAlyQa53pNM4rKg+xZ6021n/XVUCwDEzEgWqwzgflQUiHX3oxu9N7QpzFtuWBpvbkLFOb/XeG9qfOE+Z6e6hfnDXjb+9aee+cg3Hrr4/iL8ZIQVADissP6WV9dVdZZlda/nWDJGJOBMWeNCf8176MFaH5bCrKIgMP/0T/+UXLnykLz00kuyub0l733ve+Xc3h5VtD2oepH/q+CDgPcnnnhcjzWX27dv69ctBe9Dsl4AzUCPC2BnL7Qu5eDARo4wrgRoQH8S36c7FkeEHBW+PWWL6H0C/FoYYTQdZ1zni1P9s5NLyqgBWOV8JrcO70p+965cvniRrlQphVcmWIKTB47TtJYpjFJ5mg8gg2YvtqVjV8zeNBjneqGfDZMNzknDYcqZzSTGkqLUM2bOO9v3TMjkWEWIxFy/qlVBwKY4DKK0xCIK8bqWLLUlW44Y4FQZ8+XcsEUJnoUxeCU6RVmRxR6yKp6Yq1diNXK/GXB01WI6UmeuWGnaUyxPvzneOPfx9z/3Ky+Gn4iwAgCHFdYPaS2XxUI/uJcoE+fJQFYKvBgbQq+0bmsCL/5ssYA2FwsQfvbZZ+UZ/aKAyjMygDbER3VVnaX0zJRNIjVoTwEa3s0ACzgtzRHUoAwP5WsAabmq5FSBH4CL48N7GiHz4vN+F8vVWVrScDSR8XTCXjTjhxXYwJphsIESMGZebygDPr+7Lc89/bTcLl+SlR5jEjkek2wTDF+fDZBHfCLK22DTXbdkolNVrhDJJIPRkICGGeJ1zUBayz2GQUi96mgGgrK0ibPEbCid9Zxh1QxQpM1mW5uhiVjAAw04IjPMAIMtlx1dxxbFCsPO7P0KL89SpyAKgy1n5MHWebCVdam6687mqp2zOaeI40k5NwCMR/TKagB3bzDWa0jLNBt9aTo9/2r4aQgrAHBYYf0Q16Pvfbr4ype+dWex7Lp8nEewn8R8KGZtF6sFWRdUy1DTguE67wQFltt4IQ9LnrUJoBgN6AGipojJgKn1JdMSjJVAEJ2N5iB4vjeEb7I+flrL8ekJxV8AYQzwMHwBEKRsc1EtmEjkDalplpHCYhLHZ8ReLjs7OzIaT+XenVvy0je+IenRkVw+t0+3LIwHuchACufhEmOgsInEeWA0p62R95uKU7BvMRhbe+MN7xHZ4n0AiBBcAWTncz4Xs7jtcsFgCBqFdDbbDBCUyErHYMvIOeaYlgJ3xIxlkVIBfdaUFIOlCFHAe+v54RzRLCb3ZYSit8sCiDsvuIos15ijYF6khduTUKSGmWvdeJRWlsa4FVTRCeebo66Xje4lyegLjzz8/FH4aQgrAHBYYf0Q12R6uZrPZ28dHV+fL3YnozxNyTLBmACe8P+F2AnK3Yb2jxnnfFFahfoY86UY8wH4Uv7jAWAdDg9mCiDGr2BoMJGA1SGj8GBOoaCXgwVHNuMKlTO8iVGmBokri6XF6JIRigU1QPQEhyllxTSmADxFcHXKZVXC5aqSV/7qm3Lx0gV56on3yOmrb8rp0bHsT6cmKIt8P9T0SqaqBpvELLK5U/M8uwLzwtajpbd1h7zjviR4/qKSLikJesxD1k0GXLDQu0UZnZGGGHEiY3a+Vyw2y4yxIlzH0hTatTLwJkeso97HXiZ9lIrJYiGEs15uxzI1QN2EUzh27KIz60oeHy5bdWvxirh5uDZsdlikiJlIFWe4NpT10f3P215vdC1Ohq8HdXNYAYDDCuuHvKBy/eNP/psvHh3d+c71+ubTvQwM12wQMceKcSS6LCkA9pSxAXDxeyqefdYtnov+sayBFwyZ3sgWDFB6b2ELF+jYS6YKWJleC9ACGwbI6vuNMrDhkYL6QlliwUg92FjCLirLTdCEzYDNGJe0rEyy2IO1mXWcO7crVx55WIF8Ll/4i7+QRwYT/uAt9HhtXUgvMcYMZTXK3XCKYjlXr7vqKknFgiYQbo/NA1govKBx3eKV0th8YJVLuwcRIgL18RxJSQitj21MqmN7OfIbmIZzuCxHo1Sv/8v6PbJ2pEjFjVjIBfvHEfvUKD8TcJ2ptIWirIjmJRaIEVl8oh9JiiLnjUT09bhXMSeu6ZoFb+eyXpHGY6Srn08RO/y1rcHGd8NPQlgBgMMK6+9g7exe/Pbx8d3PVeXxkwqdsJ+Q2pWSYvQUvVKwYRRdayIEAfRM4avfgr8zxE1AFU7LsNQp/MBvOlgiVvpl4zAAZfSN6WiVZCytgqABqHCMmBXWWHJ9z/5gSN9ld3oiNUAN9os+voCl2cYxKUm85WU/B3suWY7FLDL6pbuXrkpPjw1l9OXxULLWxE65Mu0YgimqlxMPZo7jRkQun0tMJXStm4bFSgplujkAE6Vj/X65UiZfVNJH3CIQFV7NVctQgwgs2Pd60ziTw5MDY9G4TwqK/fFYhuMJmbdjnzaWgQI4itBx1FlfGFUAzEpDcd2Z1WTkjJHTnzsxlTYYPK04qNA28KWAq7besyUddWTzseuoOI9p29kc6ku+cu7ys4fhpyCsAMBhhfV38Y/S5UddE3+mqaJf7dJoFyySMX4KcpWCbQdv4bimmAof7ObklNj8qFjp0+fnKVbVBAC0I4kDfFbL3ibnUsF89bGeMuvBoM+xGaPOEUvZoJMYdwIDds5SfipaU7beTpK8ncwOoA6wixtkF1d0jEJ6UtVlyoIvypuvvyFvvfotefahK7K9sSlHei2ZAtIoMyOMumppKwlmi9J3ieBghNnHCR2xogwiqYTK42w4ZDgFXKqAjzDKqDAiq6g4w+ZgVbKXXDWlblCUiYOxK7NFlQACLaQJDaYT2dzbleHmFsMfGvRpez36QcNJLB32pNHXtrH4saGWlqDYHJRFbbaUiVlrQJWd531LR4qc9+PoyIypDscfnTmI4Z5xrhmjVfgIIrvGyFb2RpIOviTmERJWWAGAwwrrh732Lu53B3cv/OXN67MXq6b7x6yesnzacTSHAiLX2bhP3LOyprMep3kSd77321I8hfJwS9vE2sIaEJkHlhp1NPpAOD3mixPnvFjIWVh92fowBsf3p8K6rXmOmRc8YQwJlpCtN6Sg+tk7S2VxytLq7aMjBdNOtnb25NL5SxKdzuT0VAFysWDovVuWsqHAh41FOsjYM66X5lyV0KUrUlabUyjF/jIYftajN3NblTIcjXlOTt+jp++5mi+4zUC5OImHMgBPLxpuCADQQLdzugkYKgBH/ZxjTuLuO1ZFeo9h3BEpur722mvyrTs3RPJEtne2ec/AvpFghGukOEzPp9fLZWtzW4YUsSUs4XO7wo1KTMEV2DgygGPGEKZnqmn8PnL5wXLV/fHGxs63o7Pg5rDCCgAcVlg/1HXx0ocUy7rv/Off+hcv1HX9kTaRjciHtud5SrYKkENpF2VPI6FmcRixLdl5Erv2JG4I3NY/9V7PMKhwZmqB+VrMzSYKwvB7hiLXkn+cOUopOMEJivVtBd3l/FRBaCZlVbCsDCFVVxtpA2AXjBnsiyD+sFsyTvH43jWOOs30OVsKWjvKPNOjWE6XKxkSsCL2bYsC88UN/ZERQgBS21UWjciRHz3nxXKhTLWPmVmmLwlVzYqRgyFZf28w4iZFvPlF5D2c0RePETCBsZ91BjBVyTUdsiAkY2831vfPlJGX5u0MF7K7x0dyeHwo++e2ZXtrx4Id4OsMFXpsSmYy3U6P1ZgQ66wM3bZnjll4DpXfkvm55qTuIvd216Z/0MvyP7x69el5+AkIKwBwWGH9HS5lQc2nX/j1F773vTd+QTHzH9XO2x+iSSqtL3tGZLfrOVIG5TEjuKZwCGCLOd7OP39tFuFdik0g5P2JAeYEQXfGA1k+bpqYIzOVD2nAw4zYY++4pZMT2HPtHbDEAw3eK+/FFF7VTS77u1syfe9VOTk8kkzP7ZWvflXOAYj1NZvpSE6VtW5CPKZgmw5SyVzGUi5Gneqm0O8NyTTXPtScqwVJRiiDAicgjqNCrRlm2JcBNoCWreZBbhm/KAH7eweldazvUy4K3VQszB9aWTtYMfrWEGXtXbggG92u3D04kJu3bsu9u4eMhNzdOcdzGcKDurNeO/OKU+tjyzuYLDYPPKUo1bd1Mz3bG8rSr+kjX+33hn86Hp7/0v75x8PoUVgBgMMK60dhPfX0h64dHR/8p7I4fKbq2t2ZghQ+4M3EoSULZi8RH+6+a9jRKKM2sRV+bWufA+zjCdGrbVqWUiGUAlNkGEOS+GADCziw/3kAoZLXcYYVZhetWJkVGFgVtX6/oqKaPshdS1MMB0GSLoRCVHUi3/32mzJCFrAyyccevir7l/blwmQilQLa2/r16OYm05MGCngY1RlMpgTEGD3fNLa+c2NlaZwvSs8mXbYs3s6XqwnMUevZpvdvhkiqsxIzXbZoDWnWlImz6MWVbgoWFHX1xY36stD7gx51quC64YbSRLVs6Dkt9vbl+OhIlgrY16/fZDViOhnxOmGewihCOITptRTK4gn+OE9udmJU+18Z9ke/lff6n59Od76/u/fY7Twdl+Ffe1gBgMMK60do7V/6sdXXXv7kn7z04md/Rpnsx5I0cZEyxMUKPciW/3hjipbWIClW6lzjJkZfvAgo8uwY9eqUuh+TY6FIC2OKGJaQAFiRMyZtuIESs5/R7RIyzLQAGNbsp0IebMy6I3BK1NCwAyC/WCDEoM/jPHTpguwqyJ5TJowkoeXujnzzL16UywDa5FBO9JgjBbHj4xPpDfuyUubM0jaUy4gSRJnZOZaoI99jpcjLXzOCECCYEr85QFm6xtwwQhowItQIZ3ax8YAjVkcv506KrhLM/izA7Ec9cWCzvURWmAd2qeSDHlkyxriGvYH00p5sjLFZgFmIMm8PtBJhlvfUb5Bs3hflbfS4AfDCMn/a9LLBZ55+5rn/mKXvPQj/wsMKABxWWD/C69lnfvHGJ/7bv/6d27euPZMk9VNdDkAxwsRQgdhmUjmts07T4RhMS6EWjSGgjjYBs/2JLDHmGBLKpgh5EP/amvm2LR9n/zUyl6uISUwdTT8Wy4KMFMCMx+rOyroWQBApSMLvMaFCeLkqZb4w7+nlybE8+sgVWcwRQD+WrfPnZWu6Jd/+3nVZ6LFLsEd9z37eo0MXzgE9b/huwSjEJTb7CzMQ1yaMK8R1IshB0dZsHrMcZs3SwVoSFpRtRAtPSJzBTsUbcmDmt9b7U+gdWiWRVD3dKOhjK+xjUqfn00qDvm6USiod56+RAQzdMvrmLoeMzQAADA1JREFUK90IDUedWAwF5nhz25TgDtDZC3PMZmSCTYNzmbLrSZvn01vRavs0/MsOK6wAwGE9AOvnf+mXP/ebH/+N3z45Xf6rumzHje/xcmIm6SyNiOEEfiaG2b2d9TPJYi0UAOjG7NzWTCcYkWdPIGAgjQiCpKpqmQ5EdS4YLsIFwDMb72sM5ts6KauSLLX12B55f2UAEplqVHI0B7B88fIlifT5d+/eY2/3i1/+srz9nbclfup9MtjekeLkVI4UZKcARQVt9KOLaoWYIjLanjJTXBuyipFTDLBbwJYSCm4FP/S6E8QKFnqOcLfCfC9U2nqsGKlMMBDBeaI8jfhDvX8lZnH1JlapAnCq15ng+2ai0aI8jx5yF9Mly3VWbYA1JsaV+s7MSiIGL3Tc1JghScM2QORzjNEnx6YB4RSJw9RzvEqyaRP+VYcVVgDgsB6AlSaPz7/+zT/6vf/7mU89P1uWv4KyLwwcYMXoegqkAERFhS6mAJflWJaMG3NvYi4tpn99b5fAHJlJRAemC7FTa3MzSB1aAczyXAE+YgShT9Yj+LI0TfFVdMaaLVKvPov+g/1i7J8L16z5YiWvvPI64w+PTmY0y0j6E7n8+JPy+tvXZTBbyMN9Zb3wRe4nXhjmOJcM4RV8rgFs/eGQ34MjF5XcPhi4XBW2wYBvM84Xwq2ykGQ4EtdPZanfX+g11bgvqc3fdmmiDDji9xr9PXycK94X8SV8G/vCxiPVx1G+Fo5zmcgs8b10sF2K4aQ29yuI5cTuf+RHkmxOmnnBtV7PLMpdHf5VhxVWAOCwHpD19N/7+be//vLXP35098Zjy9XyfbF+4K8WS3EbE7pP9Xsp1citHwdibq748AFG1LbKWM39qqz9WIyYuArITaMJ7za1VjGjn9m01hWuoTKOLOSh9cprBhAo8MZ+npUCMJSj8c76upOTE5qFpEkmk+lUsqwv6WhDjupjuX73UG59/7pMlRk+vbcvN27ekGw0lg3JJEbTtzWbxigqGDqRKhutVyuWnDHqA+euPsw6qlrmCtD90ZC9WDh0QXFc4Fr0sUYZ60pBuVGW3OYJ7L1Yam5001KjV4vz9uEMbesZvpjdZLLupfNaO5+UBP/nzntrR34Gek1ofbnebg3HuSyFCnsKpy/v9C8sOg7/msMKKwBwWA/QAjDcvfvq5//of/zufyiWzb9crZYbaN3evXcow0GPat9NZYtxnhMUaDrBsnJF1XJL4PX+zz4wnoQtti4mgFU4vQOmJmeiLRtpamlBCTFS1Jo5h/WFbTSYcmjE/SEzN7KxJryu3x9yZAn90q3dPfnzP/uSwNn59r0jefrZ52SsYLw8PJaVnsPG3p7cm81kWjbsnSLsAQALJbSDMUaXEWxLZbJgpOj1rnQjMcemAjO9et4zRjG2Muz3FWj1+XqD3LAv3UjZNZTU1IzFnAuuLX6YoGqlZHcWHSi8PG8Z6ceWCK8EXq8Ux2U7Uzxz9nf9Wj6/fcfv779WmfJKv78I/5rDCisAcFgP2NrefmL+1S/9r9978cXPfNAl0a9V5QIuGnKswGUZtE6mG5tkh1TeNsZWi7bjSFIBcVJnObUtiXIrDv3N2NTNZHJK99AfBghD5Qxg4lhPa6wPamRk97b0lEbogg+DWDPn2p6L58DAItYfsVapZF128uTTT+n7xDLd2Zfv37hJW8iqWMry9EjOKaD2IWI6VNZ8blcGaS4LfRyOWytltUuDR5aai9Yp+HYsn1d5Xwo9v2VVSJ05yRX04+lEesqmHSwu80wKff8lWbwxU5t/5iS1ASnB0ZyrcL0EX099TTje+U3Q/QrBOv+XM9ldwp582zVn8ZAQt5FFJ+ZrDUW0HqHQVwUADiusAMBhPYjruQ/93Pff+tY3Pz6f3XrCZfFzcYRA90Jm8zlHihAt2Mu8PSVESPqapXdjstFXZ2lISBRi2EJ8Vk5mwHwG9bK5NfUUFNdZti17yRaaANABE40R7gDlMhh0osfPwAob640q+MLEAiKtPFaGerKUO3duy8GdIzl/8QrZ7N7ejtyNFLSaRJksvJ57cnumjFdZ77lexvMY5whWiKXwkYRwsAJzRYQgwhuSfi75dEjRmOhzORql5w6ZccU6MJytEgrI4mgNonDdMjU4/+sMik1AhUwJ947Kgw8dlvvAu15ktS46Y8ZmbhLx+NLZse3IKGo7JenJwsVZAOCwwgoAHNaDuLwQ6s9+49/989/Ps+Q9iiSTTIG0rpd0nUIfNFcAQjwfxEEVzDIiC2gAM03i9MweMe78GBFGfmB1SUMPR1BeQxADF5Cm1Nl7M5S+NQEY3hfBBVZyNTMPmGIAXAFAcM9CehGEUb0MI0SRDHq5HN87kM3tLfnetTctzEBfvzGecL65yJ3cOrgho5O78uMf/oAM9XnFcmbirqpiVCDU2bCd7A10g6Co2ugX+7mJ8wIqzPjS+MKPWnXsg8cUr/mSM/7z7XKamXBb0ZmIDeDpS/NAVlp60oHsPhCv/y5YR2A5waxCvSOomPAqYX8cgJzQlSue9fJesJoMK6wAwGE9wCBc/umnf+e/fvut195XN/XHXNvFHURKSCNCuXZVUrSE8i0ERszC9WCKMZyYkXrxGYvztVcfUG9ATZCuawJWZCjD8m3LkquBDMq1cWLqY0sAeodzFhiqgk9CZyiR8SCT1exEXn/1TWXRmbz6SoH0BKqGn3zyMTk6XjGhCIEMJ0dH0ipLf+wjf1/O7+9IUo3otwwTEgRQRE5ZsDLxlV5X1VYUiHWMezI7TTyH1+hSlpZjw0NGFNJuM4q8VaUvKcdWcq4wgtSYE1jkx7d4qWaeJWurzb/OgtfMV8Qju49+9IafmEXm3DUGmaLTJE5m4V9wWGEFAA7rAV4/9sEff/vO7YPfPDyYPaVw+SwU0ML8ARsRwogR2R86u0jkcZ4R1o2VRqmb6vg8FkgByCi/NlZWdazFQoDl+FoAGnqbibOSbcGUH+dze8Wrgg1sLOUnspCCJDYWDVMQBfT57Fg6Za+pfn80hXJ5Llv663yxJFNedvqeeaJg2MhhXcgc18T3BauN2WNO0pysHpjWsV+7thsxAEYKEl29OjtXXkpr8YGJzw9kRrI49qlbsFTsMQjELfOWWTx2ciaiYpBjZ3PUawC+L8ZyPnfZfLa56VjbeorzdqExnDrndR0FBhxWWGc/VWGF9QCu4eSR+qM//UtfyAej3y+r7rRE9i8ygzHLqx/8VYssXGBoZyFG/LIowoZzshXBmClJbc0yNFyuUEKuffBCVzc+OL6z0nNlcYbOPC+9uxbTGd4hTIr5a+O/B9MPlI9TBaXt7W05f+GCDMdj2diacpbZKX9FeRpl6EHeo7Ukxp1gegEldZRmym5TBWH7lSNDTB3C7HFL5ksnMK9edujtdp7t+s0IwJthFczl7dYVYpvVFT+vC19osPbI+0nLO0vO7RnzXYuv3smC/3p6oJX1TV3ufDIVzg+xEfFisayX4V9vWGEFAA7rAV4AgHN771meP3/hv7s4/fRqpcAJ4TNGh1oLjodXMRypELOHr3K5IvDC1rEoCn5BkIUvzPzCBYtg21iAQ+NBGN7NLQC6KmW1LOX0dC6z01NZzhdS6LFKxA42ZkABABaWslu+Hm5ZOD4EVIPxkGIxwBneD3aWmxsbMpmMCfwwqxhkqaToT+tGAS5WZoARmakIfKmjlJsJsmv2qvX7XczMYDhkpWDlkbsfyxhZljIDJ2AR2bXMMU7EADfR98n0K2+dZHqcrLX84e4+9f3/3v814P41oxI9V5axJfKlfucBO64jl562LRvuYYUVloQSdFgP+Prw8z/5VrFcffza2689VVfFo2nq53MBIjWYcM3IPvYz4X8Mx6rIepYABiYlga2lGNNx5gMtNlZTOaHQyp5X83kQd2HbCsAEe65KBUz4NXtBFz2ovXlHg/5s4wRZBZW+z9bWREYjZBondM9K0rFsbk4lSXr3DSvSmIrmVoEdrBzKZJSrI89YW+dBj+y180ETXjjF2anIet3gm2Kq724924uetQfEtrHaNLmts1CKzt0vK9+PZfQw+w4F9P154JYjWuuZX45sddYuZ9ZyZIVxbg8i5P5mp23UDzaUYYXl1/8DTWBDRDPq9joAAAAASUVORK5CYII="" class=""center""/></body></html>");
        }
    }
}
