
/**
 *@file TfrsUtil.cs 
 *
 *@brief 
 *  A utility classes using TFRSCli .NET wrapper for Toshiba Face Recognition Software Library. 
 * 
 * 
 * 
 *  @author Toshiba 
 *
 *  @date 2015/12/03 initial
 *  @date 2016/01/10 rev-1.0
 * 
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using TFRSCli;
using VideoManager;
using Core;

namespace TFRSUtil  
{

    public class TFRSUtil_ : IDisposable
    {
        public TFRSCli_ pTfrs = null;
        public bool isInitialized = false;
        public bool noLicense = false;

        /// <summary>
        /// A class containing image file name and filter of template   
        /// </summary>
        public class dicItem
        {
            private string dic_fname;
            private ulong nFilter;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="i_dicFname">[in] original image file name</param>
            /// <param name="i_nFilter">[in] filter </param>
            public dicItem(string i_dicFname,
                            ulong i_nFilter )
            {
                dic_fname = i_dicFname;
                nFilter = i_nFilter;
            }

            /// <summary>
            /// Original image filename of template
            /// </summary>
            public string DicFname
            {
                get { return dic_fname; }
                set { dic_fname = value; }
            }

            /// <summary>
            /// Filter value of this template
            /// </summary>
            public ulong NFilter
            {
                get { return nFilter; }
                set { nFilter = value; }
            }
        }


        Dictionary<string, dicItem> DList = null;

        
        /// <summary>
        /// Constructor of TFRSUtil_ 
        /// </summary>
        /// <param name="configPath"></param>
        public TFRSUtil_(string configPath) 
        {
            pTfrs = new TFRSCli_();

            if (null != pTfrs)  {
                isInitialized = true;
            }

            int iRet = pTfrs.Init(configPath, TFRSCli_.Tfrs_INIT_FULL);
            if (TFRSCli_.Tfrs_ERR_INVALID_LICENSE == iRet)
            {
                string errMsg = "error: No license for sdk";
                DebugUtil.Instance.LOG.Info(errMsg);
                Console.WriteLine(errMsg);
                isInitialized = false;
                noLicense = true;
                throw (new Exception(errMsg));
                //return;
            }
            else if (TFRSCli_.Tfrs_ERR_SUCCESS != iRet)
            {
                string errMsg = "error: pTfrs.Init(" + configPath + ",TFRSCli_.Tfrs_INIT_FULL)";
                Console.WriteLine(errMsg);
                isInitialized = false;
                throw (new Exception(errMsg));
                //return; 
            }
            Console.WriteLine("success: pTfrs.Init(" + configPath + ",TFRSCli_.Tfrs_INIT_FULL)");
            DebugUtil.Instance.LOG.Info("Sdk init successfully!");

            iRet = pTfrs.LoadDB();
            if (TFRSCli_.Tfrs_ERR_SUCCESS != iRet) {
                isInitialized = false; return;
            }

            // Commented by eno. 2016/03/08
            iRet = pTfrs.SetParameters(Encoding.ASCII.GetBytes("TFRSFACE_FORCE_DETECTION"),
                         Encoding.ASCII.GetBytes("1"));
            if (TFRSCli_.Tfrs_ERR_SUCCESS != iRet)
            {
                isInitialized = false; return;
            }

            DList = new Dictionary<string, dicItem>();

            return;
        }

        
        /// <summary>
        /// A method for disposing TFRSCli_ object
        /// </summary>
        public void Dispose() {
            pTfrs.Dispose();
        } 


        /// <summary>
        /// A method for getting TFRSCli_ object
        /// </summary>
        /// <returns>TFRSCli_ object</returns>
        public TFRSCli_ GetTFRSCli()
        {
            return pTfrs;
        }

        /// <summary>
        /// A method for one to one matching between a face image in bitmap and another face image in jpeg
        /// </summary>
        /// <param name="score">A similarity score</param>
        /// <returns>error code from TFRSCli_ object</returns>
        public int OneToOneMatching(Bitmap bmpA,
                                    Bitmap bmpB,
                                    ref float score) 
        {
            int iRet= TFRSCli_.Tfrs_ERR_SUCCESS;
            score = 0.0f;

            ImageConverter imgconv = new ImageConverter();
            byte[] curBmp = (byte[])imgconv.ConvertTo(bmpA, typeof(byte[]));

            int faceLuX = 0, faceLuY = 0, faceRbX = 0, faceRbY = 0;
            float quality = 0.0f;
            byte[] faceCurTemplate = GenTemplate(curBmp, 
                                                ref faceLuX, ref faceLuY, ref faceRbX, ref faceRbY, 
                                                ref quality, ref iRet);

            if (null != faceCurTemplate 
                && TFRSCli_.Tfrs_ERR_SUCCESS == iRet)      {
                bool isLoaded= false;
                pTfrs.IsLoadedTMPTemplate(ref isLoaded);
                if (isLoaded) { pTfrs.DeleteTMPTemplate(); }
                pTfrs.SetTMPTemplate();     // setting a template to inner buffer for matching

                byte[] inBmp = (byte[])imgconv.ConvertTo(bmpB, typeof(byte[]));

                faceLuX = faceLuY = faceRbX = faceRbY = 0;
                quality = 0.0f;
                byte[] faceInTemplate = GenTemplate(inBmp, 
                                                    ref faceLuX, ref faceLuY, ref faceRbX, ref faceRbY, 
                                                    ref quality, 
                                                    ref iRet);
                if (null != faceInTemplate
                    && TFRSCli_.Tfrs_ERR_SUCCESS == iRet)     {
                    iRet = pTfrs.VerifyMatch(null, ref score);
                }
            } 

            return iRet;
        }


        
        /// <summary>
        /// A method to enroll templates by converting facial image files readed a specified folder
        /// </summary>
        /// <param name="folder_image_gallery">[in] images folder path</param>
        /// <param name="folder_template_gallery">[in] templates folder path</param>
        /// <returns>error code from TFRSCli_ object</returns>
        public int Enroll(string folder_image_gallery,
                            string folder_template_gallery
                            )
        {
            int iRet = TFRSCli_.Tfrs_ERR_SUCCESS;

            string[] files = System.IO.Directory.GetFiles(
                                folder_image_gallery, "*.jpg", System.IO.SearchOption.AllDirectories);

            ImageConverter imgconv = new ImageConverter();
            foreach (string s in files)            {
                Bitmap bmp = new Bitmap(s);
                byte[] curBmp = (byte[])imgconv.ConvertTo(bmp, typeof(byte[]));
                bmp.Dispose();

                float quality = 0.0f;
                DETECTION_RESULT_[] dr = null;
                byte[] faceCurTemplate = GenTemplate(curBmp, 
                                                    ref dr, ref quality, ref iRet);

                if (null != faceCurTemplate)                {
                    string out_template_file = folder_template_gallery + @"\" 
                        + Path.GetFileNameWithoutExtension(s)
                        + @".out";
                    try                    {
                        File.WriteAllBytes(out_template_file, faceCurTemplate);
                        Console.WriteLine("fxs,fys,fxe,fye, lex,ley,rex,rey, lnx,lny,rnx,rny, mx,my, q, "+ 
                            dr[0].get_face_rc(0)+","+ dr[0].get_face_rc(1)+","+
                            dr[0].get_face_rc(2)+","+ dr[0].get_face_rc(3)+"," +
                            dr[0].get_points(0)+"," + dr[0].get_points(1) + "," +
                            dr[0].get_points(2) + "," + dr[0].get_points(3) + "," +
                            dr[0].get_points(4) + "," + dr[0].get_points(5) + "," +
                            dr[0].get_points(6) + "," + dr[0].get_points(7) + "," +
                            dr[0].get_points(8) + "," + dr[0].get_points(9) + "," +
                            quality);
                    }
                    catch (Exception e)                    {
                        Console.WriteLine(e.Message.ToString());
                        iRet = TFRSCli_.Tfrs_ERR_FILE_CANT_SAVE;
                    }
                }
                else { 
                    break; // Here iRet has error code.
                }
            }

            return iRet;
        }


        /// <summary>
        /// A method to enroll a template by converting facial image file to a specified template
        /// </summary>
        /// <param name="inimage">[in] input image path</param>
        /// <param name="outtemplate">[in] output template path</param>
        /// <returns>error code from TFRSCli_ object</returns>
        public int EnrollOne(string inimage,
                             string outtemplate
                            )
        {
            int iRet = TFRSCli_.Tfrs_ERR_SUCCESS;


            ImageConverter imgconv = new ImageConverter();

            Bitmap bmp = new Bitmap(inimage);
            byte[] curBmp = (byte[])imgconv.ConvertTo(bmp, typeof(byte[]));
            bmp.Dispose();

            float quality = 0.0f;
            DETECTION_RESULT_[] dr = null;
            byte[] faceCurTemplate = GenTemplate(curBmp,
                                                 ref dr, ref quality, ref iRet);

            if (null != faceCurTemplate)            {
                try                {
                    File.WriteAllBytes(outtemplate, faceCurTemplate);
                    Console.WriteLine("fxs,fys,fxe,fye, lex,ley,rex,rey, lnx,lny,rnx,rny, mx,my, q, " +
                        dr[0].get_face_rc(0) + "," + dr[0].get_face_rc(1) + "," +
                        dr[0].get_face_rc(2) + "," + dr[0].get_face_rc(3) + "," +
                        dr[0].get_points(0) + "," + dr[0].get_points(1) + "," +
                        dr[0].get_points(2) + "," + dr[0].get_points(3) + "," +
                        dr[0].get_points(4) + "," + dr[0].get_points(5) + "," +
                        dr[0].get_points(6) + "," + dr[0].get_points(7) + "," +
                        dr[0].get_points(8) + "," + dr[0].get_points(9) + "," +
                        quality);
                }
                catch (Exception e)                {
                    Console.WriteLine(e.Message.ToString());
                    iRet = TFRSCli_.Tfrs_ERR_FILE_CANT_SAVE;
                }
            }
            else            {
                Console.WriteLine("error: EnrollOne() after GenTemplate(),code:" + iRet);
                // Here iRet has error code.
            }

            return iRet;
        }


        /// <summary>
        /// A method for loading templates to inner memory database for 1 to N matching
        /// </summary>
        /// <param name="folder_template_gallery">[in] A folder including templates</param>
        /// <returns>Error codes for TFRS</returns>
        public int LoadTemplates(string folder_template_gallery)
        {
            int iRet = TFRSCli_.Tfrs_ERR_SUCCESS;

            string[] files =
                System.IO.Directory.GetFiles(folder_template_gallery,
                                            "*.out", System.IO.SearchOption.AllDirectories);

            // loading templates
            Console.WriteLine("Loading templates");
            foreach (string s in files)            {
                byte[] a_dic = File.ReadAllBytes(s);
                if (null != a_dic)                {
                    // import a template to engine's memory database.
                    string id = Path.GetFileNameWithoutExtension(s);
                    pTfrs.SetDicInfoFromDicBuff(a_dic, id);
                    ulong nFilter = 0xffffffff;
                    dicItem di = new dicItem(s, nFilter);
                    DList.Add(id, di);
                }
            }

            return iRet;
        }

        /// <summary>
        /// A method for adding templates to inner memory database for 1 to N matching
        /// </summary>
        /// <param name="folder_template_gallery">[in] the template file full name, use its name as template ID</param>
        /// <returns>Error codes for TFRS</returns>
        public int AddTemplate(string templatePath)
        {
            int iRet = TFRSCli_.Tfrs_ERR_SUCCESS;
            // Add new template
            Console.WriteLine("Loading templates");
            byte[] a_dic = File.ReadAllBytes(templatePath);
            if (null != a_dic)
            {
                // import a template to engine's memory database.
                string id = Path.GetFileNameWithoutExtension(templatePath);
                pTfrs.SetDicInfoFromDicBuff(a_dic, id);
                ulong nFilter = 0xffffffff;
                dicItem di = new dicItem(templatePath, nFilter);
                DList.Add(id, di);
            }

            return iRet;
        }

        /// <summary>
        /// clear all templates, write by myself
        /// </summary>
        public void ClearAllTemplates()
        {
            foreach (KeyValuePair<string, dicItem> entry in DList)
            {
                pTfrs.DeleteDBTemplate(entry.Key);
            }
//            int num = 0;
 //           int temp = pTfrs.GetNumberOfTemplateLoad(ref num);
            DList.Clear();
        }

        /// <summary>
        /// A method for One to N matching
        /// </summary>
        /// <param name="a_probe_bmp">[in] a probe in Bitmap data</param>
        /// <param name="nCandidates">[in] number of candidates to output</param>
        /// <param name="result_list">[out] list of string "TemplateID, score, template-filename"</param>
        /// <returns>error code of TFRS </returns>
        public int OneToN(Bitmap a_probe_bmp,
                          int nCandidates,
                          ref List<string> result_list
                            )
        {
            DebugUtil.Instance.LOG.Debug("Function Entered: OneToN");
            int iRet = TFRSCli_.Tfrs_ERR_SUCCESS;

            int num = 0;
            iRet = pTfrs.GetNumberOfTemplateLoad(ref num);
            DebugUtil.Instance.LOG.Debug("Number Of Template Loaded: " + num);
            if (TFRSCli_.Tfrs_ERR_SUCCESS != iRet) { return iRet; }
            if (num <= 0)          {
                iRet = TFRSCli_.Tfrs_ERR_ISNOT_LOAD_PERSONAL_DIC;
                return iRet;
            }

            int numCandidates = System.Math.Min(nCandidates, DList.Count);

            // 1 to N matching x M inputs
            Console.WriteLine("Start 1 to N matching");
            ImageConverter imgconv = new ImageConverter();

            byte[] curBmp = (byte[])imgconv.ConvertTo(a_probe_bmp, typeof(byte[]));

            int faceLuX = 0, faceLuY = 0, faceRbX = 0, faceRbY = 0;
            float quality = 0.0f;
            DebugUtil.Instance.LOG.Debug("GenTemplate Start");
            byte[] faceCurTemplate =
                GenTemplate(curBmp,
                            ref faceLuX, ref faceLuY, ref faceRbX, ref faceRbY,
                            ref quality,
                            ref iRet);
            DebugUtil.Instance.LOG.Debug("GenTemplate Finished");
            if (null != faceCurTemplate)            
            {
                //identifying with DB face(s)
                RECOG_RESULT_[] recogresult = new RECOG_RESULT_[DList.Count + 1];
                for (int i = 0; i < DList.Count; i++) 
                { 
                    recogresult[i] = new RECOG_RESULT_(); 
                }

                try                
                {
                    DebugUtil.Instance.LOG.Debug("IdentifyMatch");
                    iRet = pTfrs.IdentifyMatch(numCandidates,
                                                recogresult,
                                                true,
                                                TFRSCli_.Tfrs_IDENTIFY_NORMAL_MODE);
                    if (TFRSCli_.Tfrs_ERR_SUCCESS == iRet)                    
                    {
                        DebugUtil.Instance.LOG.Debug("Add result to listview");
                        for (int i = 0; i < numCandidates; i++)                        
                        {
                            /*string aResult =
                                recogresult[i].TemplateID + "," +
                                recogresult[i].score + "," +
                                (DList[recogresult[i].TemplateID]).DicFname;*/
                           result_list.Add(recogresult[i].TemplateID + "," + recogresult[i].score);
                           DebugUtil.Instance.LOG.Debug("TemplateID=" + recogresult[i].TemplateID + ",score=" + recogresult[i].score);
                        }
                    }
                    else                    
                    {
                        Console.WriteLine("err IdentifyMatch()," + iRet);
                        DebugUtil.Instance.LOG.Debug("IdentifyMatch Error, Error Code=" + iRet);
                    }
                }
                catch (Exception e)                
                {
                    Console.WriteLine(e.ToString());
                }
            }
            DebugUtil.Instance.LOG.Debug("Function Entered: OneToN");
            return iRet;
        }


        /// <summary>
        /// A method for matching in MtoN manner
        /// </summary>
        /// <param name="probes">[in] A strings array showing path to probe image files </param>
        /// <param name="nCandidates">[in] Number of candidates to present</param>
        /// <param name="result_list">[out] A strings list whose each string contains path_to_a_probe, tempate-ID, similarity, template-filename                                 </param>
        /// <returns>error code from TFRSCli_ object</returns>
        public int MToN(string [] probes, 
                        int nCandidates,
                        ref List<string> result_list
                        )
        {
            int iRet = TFRSCli_.Tfrs_ERR_SUCCESS;

            int num=0;
            iRet= pTfrs.GetNumberOfTemplateLoad(ref num);
            if (TFRSCli_.Tfrs_ERR_SUCCESS != iRet) { return iRet; }
            if (num <= 0) { 
                iRet = TFRSCli_.Tfrs_ERR_ISNOT_LOAD_PERSONAL_DIC;
                return iRet;
            }

            int numCandidates = System.Math.Min(nCandidates, DList.Count);

            // 1 to N matching x M inputs
            Console.WriteLine("Start 1 to N matching x M");
            ImageConverter imgconv = new ImageConverter();
            foreach (string p in probes)       {
                Bitmap bmp = new Bitmap(p);
                byte[] curBmp = (byte[])imgconv.ConvertTo(bmp, typeof(byte[]));
                bmp.Dispose();

                int faceLuX = 0, faceLuY = 0, faceRbX = 0, faceRbY = 0;
                float quality = 0.0f;
                byte[] faceCurTemplate = 
                    GenTemplate(curBmp, 
                                ref faceLuX, ref faceLuY, ref faceRbX, ref faceRbY, 
                                ref quality, 
                                ref iRet);

                if (null != faceCurTemplate)  {
            		//identifying with DB face(s)
		            RECOG_RESULT_[] recogresult= new RECOG_RESULT_[DList.Count+1];
                    for (int i = 0; i < DList.Count; i++) { recogresult[i] = new RECOG_RESULT_(); }

                    try                    {
                        iRet = pTfrs.IdentifyMatch(numCandidates, 
                                                    recogresult, 
                                                    true, 
                                                    TFRSCli_.Tfrs_IDENTIFY_NORMAL_MODE);

                        if (TFRSCli_.Tfrs_ERR_SUCCESS == iRet)  {
                            for (int i = 0; i < numCandidates; i++)  {
                                string aResult= 
                                    p+","+
                                    recogresult[i].TemplateID+","+
                                    recogresult[i].score+","+
                                    (DList[recogresult[i].TemplateID]).DicFname;
                                result_list.Add(aResult);
                            }
                        }
                        else      {
                            Console.WriteLine("err IdentifyMatch()," + iRet);
                        }
                    }
                    catch (Exception e) { 
                        Console.WriteLine(e.ToString()); 
                    }
                }

            }
            return iRet;
        }


        /// <summary>
        /// A method for detecting facial attributes 
        /// </summary>
        /// <param name="bmp">[in] a probe image in Bitmap format data</param>
        /// <param name="iRet">[out] error code of TFRS</param>
        /// <returns>An array of DETECTION_RESULT_</returns>
        public DETECTION_RESULT_[]
            Attribute(Bitmap bmp,
                      ref int iRet
                      )
        {
            iRet = TFRSCli_.Tfrs_ERR_SUCCESS;
            DETECTION_RESULT_[] drOut = null;

            ImageConverter imgconv = new ImageConverter();
            byte[] imageBinary = (byte[])imgconv.ConvertTo(bmp, typeof(byte[]));

            BasicImageFileCli_ bifTmp = new BasicImageFileCli_();
            bool bret = bifTmp.image_load_jpg_bmp_binary(imageBinary,
                                                (uint)imageBinary.GetLength(0));
            if (bret)            {
                if (bifTmp.bytetype == 24)  {
                    bifTmp.gray_image();
                }
                Int32 detect_num = CachedMap.Instance.numMaxFaces;    // Only one==> maximum faces in each photo. //enoenoeno
                int num_dr = 0;
                DETECTION_RESULT_[] dr = getIniializedDetectionResult(ref num_dr);
                iRet = pTfrs.FaceDetect(bifTmp, dr, ref detect_num);
                if (TFRSCli_.Tfrs_ERR_SUCCESS != iRet)                {
                    Console.WriteLine(@"pTFRS->TFRS_FaceDetect()!= TFRS_ERR_SUCCESS");
                    return null;
                }

                drOut = new DETECTION_RESULT_[detect_num];
                for (int i = 0; i < detect_num; i++)  {
                    iRet = pTfrs.GetFaceAttr(bifTmp, dr[i]);
                    if (TFRSCli_.Tfrs_ERR_SUCCESS == iRet)  {
                        drOut[i] = dr[i];
                    }
                }
            }
            else { 
                iRet = TFRSCli_.Tfrs_ERR_NOT_SUPPORTED; // error in image_load_jpg_bmp_binary()
            }
            return drOut;
        }


        /// <summary>
        /// A method for initializing DETECTION_RESULT_ array
        /// </summary>
        /// <param name="num">[in] number of elements </param>
        /// <returns>an initialized DETECTION_RESULT_ array </returns>
        public DETECTION_RESULT_[] getIniializedDetectionResult(ref int num)
        {
            num = TFRSCli_.Tfrs_MAX_DETECTION_NUM;
            DETECTION_RESULT_[] dr = new DETECTION_RESULT_[TFRSCli_.Tfrs_MAX_DETECTION_NUM];

            for (int ii = 0; ii < TFRSCli_.Tfrs_MAX_DETECTION_NUM; ii++)  {
                dr[ii] = new DETECTION_RESULT_();
            }
            return dr;
        }


        /// <summary>
        /// A method for generating a template from a image binary(image data with header like bmp/jpeg)
        /// </summary>
        /// <param name="imageBinary">[in] a byte array contains an image whose format is bmp or jpeg </param>
        /// <param name="faceLuX">[out] x coordinate of left upper corner of face region</param>
        /// <param name="faceLuY">[out] y coordinate of left upper corner of face region</param>
        /// <param name="faceRbX">[out] x coordinate of right bottom corner of face region</param>
        /// <param name="faceRbY">[out] y coordinate of right bottom corner of face region</param>
        /// <param name="quality">[out] a quality value</param>
        /// <param name="errCode">[out] error code of TFRSCli object </param>
        /// <returns>byte array contains template data for a person </returns>
        public byte[] GenTemplate(byte[] imageBinary,
                                    ref int faceLuX,
                                    ref int faceLuY,
                                    ref int faceRbX,
                                    ref int faceRbY,
                                    ref float quality,
                                    ref int errCode
                                    )
        {
            byte[] tmplt = null;

            DebugUtil.Instance.LOG.Debug("image_load_jpg_bmp_binary");
            BasicImageFileCli_ bifTmp = new BasicImageFileCli_();
            bool bret = bifTmp.image_load_jpg_bmp_binary(imageBinary,
                                                (uint)imageBinary.GetLength(0));
            
            if (bret)            
            {
                if (bifTmp.bytetype == 24)                
                {
                    DebugUtil.Instance.LOG.Debug("gray_image");
                    bifTmp.gray_image();
                }
                Int32 detect_num = 1;    // Only one face should be detected in each photo. //enoenoeno
                int num_dr = 0;
                DebugUtil.Instance.LOG.Debug("getIniializedDetectionResult");
                DETECTION_RESULT_[] dr = getIniializedDetectionResult(ref num_dr);

                DebugUtil.Instance.LOG.Debug("FaceDetect");
                int iRetVal = pTfrs.FaceDetect(bifTmp, dr, ref detect_num);
                if (TFRSCli_.Tfrs_ERR_SUCCESS != iRetVal)                
                {
                    Console.WriteLine(@"pTFRS->TFRS_FaceDetect()!= TFRS_ERR_SUCCESS");
                    errCode = iRetVal;
                    return null;
                }

                if (detect_num > 0)                
                {
                    faceLuX = dr[0].get_face_rc(0);
                    faceLuY = dr[0].get_face_rc(1);
                    faceRbX = dr[0].get_face_rc(2);
                    faceRbY = dr[0].get_face_rc(3);

                    QUALITY_REPORT_ qr = new QUALITY_REPORT_();
                    DebugUtil.Instance.LOG.Debug("GetQualityAssessment");
                    iRetVal = pTfrs.GetQualityAssessment(bifTmp, dr[0], qr);

                    if (TFRSCli_.Tfrs_ERR_SUCCESS == iRetVal)                    
                    {
                        float min = -100.0f;
                        float max = 200.0f;
                        quality = (qr.face_detection_score - min) * 100.0f / (max - min);
                    }

                    // extracting features from first face
                    DebugUtil.Instance.LOG.Debug("MakeFeature");
                    iRetVal = pTfrs.MakeFeature(bifTmp, dr[0]);
                    if (iRetVal != TFRSCli_.Tfrs_ERR_SUCCESS)                    
                    {
                        Console.WriteLine("pTFRS->TFRS_MakeFeature() != TFRS_ERR_SUCCESS");
                        errCode = iRetVal;
                        return null;
                    }
                    else                    
                    {
                        // Obtaining feature
                        DebugUtil.Instance.LOG.Debug("GetDicBuffFromDicInfo");
                        tmplt = pTfrs.GetDicBuffFromDicInfo(ref errCode);
                        if (tmplt == null)                        
                        {
                            Console.WriteLine("pDicBiff == null");
                        }
                    }
                }
                else                
                {
                    Console.WriteLine("detect_num <= 0");
                }
            }
            else            
            {
                Console.WriteLine("bifTmp.image_load_jpg_bmp_binary() failed");
                errCode = TFRSCli_.Tfrs_ERR_UNKNOWN_ERROR;
            }
            return tmplt;
        }


        /// <summary>
        /// A method for generating a template from a image binary(image data with header like bmp/jpeg)
        /// </summary>
        /// <param name="imageBinary">[in] a byte array contains an image whose format is bmp or jpeg</param>
        /// <param name="dr">[out] An array of DETECTION_RESULT_ </param>
        /// <param name="quality">[out] a quality value</param>
        /// <param name="errCode">[out] error code of TFRSCli object </param>
        /// <returns>byte array contains template data for a person </returns>
        public byte[] GenTemplate(byte[] imageBinary,
                                    ref DETECTION_RESULT_[] dr,
                                    ref float quality,
                                    ref int errCode
                                    )
        {
            byte[] tmplt = null;

            BasicImageFileCli_ bifTmp = new BasicImageFileCli_();
            bool bret = bifTmp.image_load_jpg_bmp_binary(imageBinary,
                                                (uint)imageBinary.GetLength(0));
            if (bret)            {
                if (bifTmp.bytetype == 24)                {
                    bifTmp.gray_image();
                }
                Int32 detect_num = 1;    // Only one face should be detected in each photo. //enoenoeno
                int num_dr = 0;
                dr = getIniializedDetectionResult(ref num_dr);

                int iRetVal = pTfrs.FaceDetect(bifTmp, dr, ref detect_num);

                if (TFRSCli_.Tfrs_ERR_SUCCESS != iRetVal)                {
                    Console.WriteLine(@"pTFRS->TFRS_FaceDetect()!= TFRS_ERR_SUCCESS");
                    errCode = iRetVal;
                    return null;
                }
                if (detect_num > 0)                {
                    QUALITY_REPORT_ qr = new QUALITY_REPORT_();
                    iRetVal = pTfrs.GetQualityAssessment(bifTmp, dr[0], qr);

                    if (TFRSCli_.Tfrs_ERR_SUCCESS == iRetVal)                    {
                        float min = -100.0f;
                        float max = 200.0f;
                        quality = (qr.face_detection_score - min) * 100.0f / (max - min);
                    }

                    // extracting features from first face
                    iRetVal = pTfrs.MakeFeature(bifTmp, dr[0]);
                    if (iRetVal != TFRSCli_.Tfrs_ERR_SUCCESS)              {
                        Console.WriteLine("pTFRS->TFRS_MakeFeature() != TFRS_ERR_SUCCESS");
                        errCode = iRetVal;
                        return null;
                    }
                    else                    {
                        // Obtaining feature
                        tmplt = pTfrs.GetDicBuffFromDicInfo(ref errCode);
                        if (tmplt == null)                        {
                            Console.WriteLine("pDicBiff == null");
                        }
                    }
                }
                else                {
                    Console.WriteLine("detect_num <= 0");
                }
            }
            else            {
                Console.WriteLine("bifTmp.image_load_jpg_bmp_binary() failed");
                errCode = TFRSCli_.Tfrs_ERR_UNKNOWN_ERROR;
            }
            return tmplt;
        }

    }
}
