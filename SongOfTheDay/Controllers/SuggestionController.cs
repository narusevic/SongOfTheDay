using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.Identity;
using SongOfTheDay.Models;

namespace SongOfTheDay.Controllers
{
    public class SuggestionController : Controller
    {
        public ActionResult Index()
        {
            return PartialView("Index");
        }

        public ActionResult SuggestVideo(string suggestedVideoUrl, string suggestedVideoOpinion, string suggestedVideoName)
        {
            if ((suggestedVideoUrl.Contains("www.youtube.com") || suggestedVideoUrl.Contains("www.youtu.be") ||
                 suggestedVideoUrl.ToLower().Contains("soundcloud")) && (suggestedVideoUrl.Length >= 27) &&
                suggestedVideoUrl != null)
            {
                using (var db = new DbConnection())
                {
                    var client = new WebClient();

                    string name = "";
                    string imageUrl = "";
                    string SCid = "";
                    string SCname = "";

                    if (!suggestedVideoUrl.ToLower().Contains("soundcloud"))
                    {

                        var downloadString =
                            client.DownloadString("https://gdata.youtube.com/feeds/api/videos/" +
                                                  GetVideoApi(suggestedVideoUrl) + "?v=2");
                        var infoList = downloadString.ToList();


                        #region Name

                        var title = downloadString.IndexOf("<media:title type='plain'>");

                        var nameList = new List<char>();

                        for (var i = title; i < infoList.Count; i++)
                        {
                            if (infoList[i] == 62)
                            {
                                for (var x = i + 1; x < infoList.Count; x++)
                                {
                                    if (infoList[x] != 60)
                                    {
                                        nameList.Add(infoList[x]);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                break;
                            }
                        }

                        for (var i = 0; i < nameList.Count; i++)
                        {
                            name = name + nameList[i];
                        }

                        #endregion

                        #region image

                        var image = downloadString.IndexOf("<media:thumbnail");

                        var imageList = new List<char>();

                        for (var i = image; i < infoList.Count; i++)
                        {
                            if (infoList[i] == 39)
                            {
                                for (var x = i + 1; x < infoList.Count; x++)
                                {
                                    if (infoList[x] != 39)
                                    {
                                        imageList.Add(infoList[x]);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                break;
                            }
                        }

                        for (var i = 0; i < imageList.Count; i++)
                        {
                            imageUrl = imageUrl + imageList[i];
                        }

                        #endregion

                        suggestedVideoUrl = ConvertVideoUrl(suggestedVideoUrl);
                    }
                    else
                    {
                        var downloadString =
                            client.DownloadString("http://soundcloud.com/oembed?format=js&url=" + suggestedVideoUrl +
                                                  "&iframe=true");
                        var infoList = downloadString.ToList();

                        #region SC id

                        var idList = new List<char>();

                        var videoId = downloadString.LastIndexOf("tracks%2F");

                        for (var i = videoId + 9; i < infoList.Count; i++)
                        {
                            if (infoList[i] != 92)
                            {
                                idList.Add(infoList[i]);
                            }
                            else
                            {
                                break;
                            }
                        }

                        for (var i = 0; i < idList.Count; i++)
                        {
                            SCid = SCid + idList[i];
                        }

                        #endregion

                        #region SC name

                        var nameList = new List<char>();

                        var videoName = downloadString.LastIndexOf("\"title\":\"");

                        for (var i = videoName + 9; i < infoList.Count; i++)
                        {
                            if (infoList[i] != 34)
                            {
                                nameList.Add(infoList[i]);
                            }
                            else
                            {
                                break;
                            }
                        }

                        for (var i = 0; i < nameList.Count; i++)
                        {
                            SCname = SCname + nameList[i];
                        }

                        #endregion

                        suggestedVideoUrl =
                            ("https://w.soundcloud.com/player/?url=https%3A//api.soundcloud.com/tracks/" + SCid +
                             "&amp;auto_play=false&amp;hide_related=false&amp;visual=true");

                    }

                    var newSuggestion = new Suggestion
                    {
                        Url = suggestedVideoUrl,
                        Opinion = suggestedVideoOpinion.Replace("\r\n", Environment.NewLine),
                        Name = (name != "") ? name : SCname,
                        User = suggestedVideoName,
                        Image = (imageUrl != "") ? imageUrl : "http://www.artscouncil.org.uk/media/images/soundcloud_thumbnail_logo.gif",

                    };

                    db.Suggestions.Add(newSuggestion);

                    db.SaveChanges();
                }

            }

            //Task.Delay(2000);

            System.Threading.Thread.Sleep(1000);

            return RedirectToAction("Index");

        }

        public ActionResult AdminIndex()
        {
            return PartialView("AdminIndex");
        }

        public static string GetSuggestedVideo()
        {
            using (var db = new DbConnection())
            {
                var video = db.Suggestions.OrderByDescending(x => x.SuggestionId).FirstOrDefault();
                if (video != null)
                {
                    return video.Url;
                }
            }

            return null;
        }

        public ActionResult SuggestionsArchive()
        {
            List<Suggestion> suggestions;

            using (var db = new DbConnection())
            {
                suggestions = db.Suggestions.OrderByDescending(x => x.SuggestionId).ToList();
            }

            suggestions.ForEach(u => u.User = !string.IsNullOrEmpty(u.User) ? ("by " + u.User) : "Anonymous" );
            suggestions.ForEach(n => n.Name = (n.Name.Length > 40) ? (n.Name.Substring(0, 37) + "...") : n.Name);


            return PartialView(suggestions);
        }

        public ActionResult EditSuggestion(int id)
        {
            var suggestion = new Suggestion();

            using (var db = new DbConnection())
            {
                suggestion = db.Suggestions.Find(id);
            }

            return View(suggestion);
        }

        public ActionResult SaveSuggestion(int suggestionId, string editLink, string editName, string editUser, string editInfo)
        {

            using (var db = new DbConnection())
            {
                db.Suggestions.Find(suggestionId).Opinion = editInfo.Replace("\r\n", Environment.NewLine);
                db.Suggestions.Find(suggestionId).Name = editName;
                db.Suggestions.Find(suggestionId).Url = ConvertVideoUrl(editLink);
                db.Suggestions.Find(suggestionId).User = editUser;

                db.SaveChanges();
            }
            
            return RedirectToAction("AdminIndex", "Suggestion");
        }

        public ActionResult RemoveSuggestion(int id)
        {
            using (var db = new DbConnection())
            {
                var suggestion = db.Suggestions.FirstOrDefault(x => x.SuggestionId == id);
                if (suggestion != null)
                {
                    db.Suggestions.Remove(suggestion);

                    db.SaveChanges();
                }
            }

            return RedirectToAction("AdminIndex", "Suggestion");
        }

        public ActionResult AddToMain(int id)
        {
            using (var db = new DbConnection())
            {
                string userId = User.Identity.GetUserId();

                var suggestion = db.Suggestions.FirstOrDefault(x => x.SuggestionId == id);

                var video = new Video()
                {
                    Link = suggestion.Url,
                    Name = suggestion.Name,
                    Information = suggestion.Opinion.Replace("\r\n", Environment.NewLine),
                    Image = suggestion.Image,
                    Date = DateTime.Today,
                    User = db.ApplicationUsers.Find(userId)
                };

                db.Videos.Add(video);

                db.SaveChanges();
            }

            return RedirectToAction("RemoveSuggestion", "Suggestion", new { id });
        }

        public string ConvertVideoUrl(string videoLink)
        {
            const string pattern = @"(?:https?:\/\/)?(?:www\.)?(?:(?:(?:youtube.com\/watch\?[^?]*v=|youtu.be\/)([\w\-]+))(?:[^\s?]+)?)";
            const string replacement = "http://www.youtube.com/embed/$1";
            var rgx = new Regex(pattern);

            var videoLinkEmbed = rgx.Replace(videoLink, replacement) + "?hd=1&iv_load_policy=3";

            return videoLinkEmbed;
        }

        public string GetVideoApi(string videoLink)
        {
            const string pattern = @"(?:https?:\/\/)?(?:www\.)?(?:(?:(?:youtube.com\/watch\?[^?]*v=|youtu.be\/)([\w\-]+))(?:[^\s?]+)?)";
            const string replacementId = "$1";
            var rgx = new Regex(pattern);

            var videoUrlId = rgx.Replace(videoLink, replacementId);

            return videoUrlId;
        }
    }
}