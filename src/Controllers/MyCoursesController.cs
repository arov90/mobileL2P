﻿using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http;
using System;
using L2PAPIClient;
using System.Net;
using L2PAPIClient.DataModel;
using System.Collections.Generic;
using Grp.L2PSite.MobileApp.Services;
using System.Threading.Tasks;

namespace Grp.L2PSite.MobileApp.Controllers
{
    public class MyCoursesController : Controller
    {
        [HttpGet] // Get Method to retrieve the course What's New Page
        public async Task<IActionResult> WhatsNew(string cId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context))
                {
                    Context.Session.SetString("CourseId", cId);
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["CourseWhatsNew"] = await L2PAPIClient.api.Calls.L2PwhatsNewSinceAsync(cId, 180000);

                    L2PAssignmentList assnList = await L2PAPIClient.api.Calls.L2PviewAllAssignments(cId);
                    List<L2PAssignmentElement> assignments = new List<L2PAssignmentElement>();
                    if (assnList.dataSet != null)
                    {
                        assignments = assnList.dataSet;
                        // Sort by publish date desc
                        assignments.Sort((x, y) => y.assignmentPublishDate.CompareTo(x.assignmentPublishDate));
                    }
                    ViewData["Assignments"] = assignments;
                    return View();
                }
                else
                {
                    return RedirectToAction(nameof(AccountController.Login), "Account");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = ex.Message });
            }
        }

        [HttpGet] // Get Method to show the subject info of a course.
        public IActionResult ShowSubject(string cId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["CourseInfo"] = L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId).Result;
                    return View();
                }
                else
                {
                    return RedirectToAction(nameof(AccountController.Login), "Account");
                }
            }
            catch (Exception ex) {
                return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = ex.Message });
            }
        }

        public async Task<IActionResult> LearningMaterials(String cId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    L2PLearningMaterialList lmList = await L2PAPIClient.api.Calls.L2PviewAllLearningMaterials(cId);
                    List<L2PLearningMaterialElement> learningMaterial = new List<L2PLearningMaterialElement>();
                    if (lmList.dataSet != null)
                    {
                        learningMaterial = lmList.dataSet;
                    }
                    ViewData["CourseLearningMaterials"] = learningMaterial;
                    return View();
                }
                else
                {
                    return RedirectToAction(nameof(AccountController.Login), "Account");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = ex.Message });
            }

            //return View();
        }
        public IActionResult SharedDocuments()
        {
            return View();
        }

        [HttpGet] // Get Method to show all the hyperlinks of a course
        public async Task<IActionResult> Hyperlinks(String cId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    L2PHyperlinkList hpList = await L2PAPIClient.api.Calls.L2PviewAllHyperlinks(cId);
                    List<L2PHyperlinkElement> hyperlinks = new List<L2PHyperlinkElement>();
                    if (hpList.dataSet != null)
                    {
                        hyperlinks = hpList.dataSet;
                    }
                    ViewData["CourseHyperlinks"] = hyperlinks;
                    return View();
                }
                else
                {
                    return RedirectToAction(nameof(AccountController.Login), "Account");
                }
            }
            catch (Exception ex) {
                return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = ex.Message });
            }
        }

        public IActionResult Literature()
        {
            return View();
        }
        public IActionResult MediaLibrary()
        {
            return View();
        }

        public IActionResult Assignments()
        {
            return View();
        }

        public IActionResult Announcement()
        {
            return View();
        }
        public IActionResult AddAnnouncement()
        {
            return View();
        }
        public IActionResult AddLiterature()
        {
            return View();
        }
        public IActionResult AddHyperlink()
        {
            return View();
        }

        // Function used to download files from the L2P Client API
        public ActionResult Downloads(string url, string filename)
        {
            try
            {
                string callURL = Config.L2PEndPoint + "/downloadFile/" + filename + "?accessToken=" + Config.getAccessToken() + "&cid=" + Context.Session.GetString("CourseId") + "&downloadUrl=|" + url;
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(callURL);
                myHttpWebRequest.MaximumAutomaticRedirections = 1;
                myHttpWebRequest.AllowAutoRedirect = true;
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                return File(myHttpWebResponse.GetResponseStream(), myHttpWebResponse.ContentType, filename);
            }
            catch (Exception ex)
            {
                ViewData["error"] = ex.Message;
                return RedirectToAction(nameof(HomeController.Error), "Error");
            }
        }

    }
}
