﻿//    Copyright (C) 2020, Luke Emmet 

//    Email: luke [dot] emmet [at] gmail [dot] com

//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.

//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.

//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//===================================================

using GemiNaut.Serialization.Commandline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GemiNaut.GopherNavigator;

namespace GemiNaut
{

    internal static class ConverterService
    {

        //convert text to GMI for raw text
        public static Tuple<int, string, string> HtmlToGmi(string rawPath, string outPath)
        {
            var appDir = System.AppDomain.CurrentDomain.BaseDirectory;
            var finder = new ResourceFinder();

            //allow for rebol and converters to be in sub folder of exe (e.g. when deployed)
            //otherwise we use the development ones which are version controlled
            var converterPath = finder.LocalOrDevFile(appDir, @"HtmlToGmi", @"..\..\..\HtmlToGmi", "html2gmi.exe");

            //for some unknown reason, the -m flag (numbered citations) must not be last when calling from this context
            //-e (show embedded images as links)
            //-n (number links)
            var command = String.Format("\"{0}\" -mne -i \"{1}\" -o \"{2}\"",
                converterPath,
                rawPath,
                outPath

                );

            var execProcess = new ExecuteProcess();

            var result = execProcess.ExecuteCommand(command);

            return (result);
        }


        //convert text to GMI for raw text
        public static Tuple<int, string, string> TextToGmi(string rawPath, string outPath)
        {
            var appDir = System.AppDomain.CurrentDomain.BaseDirectory;
            var finder = new ResourceFinder();

            //allow for rebol and converters to be in sub folder of exe (e.g. when deployed)
            //otherwise we use the development ones which are version controlled
            var rebolPath = finder.LocalOrDevFile(appDir, @"Rebol", @"..\..\Rebol", "r3-core.exe");
            var scriptPath = finder.LocalOrDevFile(appDir, @"GmiConverters", @"..\..\GmiConverters", "TextAsIs.r3");


            //due to bug in rebol 3 at the time of writing (mid 2020) there is a known bug in rebol 3 in 
            //working with command line parameters, so we need to escape quotes
            //see https://stackoverflow.com/questions/6721636/passing-quoted-arguments-to-a-rebol-3-script
            //also hypens are also problematic, so we base64 each param and unpack in the script
            var command = String.Format("\"{0}\" -cs \"{1}\" \"{2}\" \"{3}\"  ",
                rebolPath,
                scriptPath,
                Base64Service.Base64Encode(rawPath),
                Base64Service.Base64Encode(outPath)

                );

            var execProcess = new ExecuteProcess();

            var result = execProcess.ExecuteCommand(command);

            return (result);
        }


        //convert GopherText to GMI and save to outpath
        public static Tuple<int, string, string> GophertoGmi(string gopherPath, string outPath, string uri, GopherParseTypes parseType)
        {
            var appDir = System.AppDomain.CurrentDomain.BaseDirectory;
            var finder = new ResourceFinder();

            var parseScript = (parseType == GopherParseTypes.Map) ? "GophermapToGmi.r3" : "GophertextToGmi.r3";

            //allow for rebol and converters to be in sub folder of exe (e.g. when deployed)
            //otherwise we use the development ones which are version controlled
            var rebolPath = finder.LocalOrDevFile(appDir, @"Rebol", @"..\..\Rebol", "r3-core.exe");
            var scriptPath = finder.LocalOrDevFile(appDir, @"GmiConverters", @"..\..\GmiConverters", parseScript);

            //due to bug in rebol 3 at the time of writing (mid 2020) there is a known bug in rebol 3 in 
            //working with command line parameters, so we need to escape quotes
            //see https://stackoverflow.com/questions/6721636/passing-quoted-arguments-to-a-rebol-3-script
            //also hypens are also problematic, so we base64 each parameter and unpack it in the script
            var command = String.Format("\"{0}\" -cs \"{1}\" \"{2}\" \"{3}\" \"{4}\" ",
                rebolPath,
                scriptPath,
                Base64Service.Base64Encode(gopherPath),
                Base64Service.Base64Encode(outPath),
                Base64Service.Base64Encode(uri)

                );

            var execProcess = new ExecuteProcess();

            var result = execProcess.ExecuteCommand(command);

            return (result);

        }

        //convert GMI to HTML for display and save to outpath
        public static Tuple<int, string, string> GmiToHtml(string gmiPath, string outPath, string uri, SiteIdentity siteIdentity, string theme, bool showWebHeader)
        {
            var appDir = System.AppDomain.CurrentDomain.BaseDirectory;
            var finder = new ResourceFinder();

            //allow for rebol and converters to be in sub folder of exe (e.g. when deployed)
            //otherwise we use the development ones which are version controlled
            var rebolPath = finder.LocalOrDevFile(appDir, @"Rebol", @"..\..\Rebol", "r3-core.exe");
            var scriptPath = finder.LocalOrDevFile(appDir, @"GmiConverters", @"..\..\GmiConverters", "GmiToHtml.r3");

            var identiconUri = new System.Uri(siteIdentity.IdenticonImagePath());
            var fabricUri = new System.Uri(siteIdentity.FabricImagePath());

            //due to bug in rebol 3 at the time of writing (mid 2020) there is a known bug in rebol 3 in 
            //working with command line parameters, so we need to escape quotes
            //see https://stackoverflow.com/questions/6721636/passing-quoted-arguments-to-a-rebol-3-script
            //also hypens are also problematic, so we base64 each param and unpack in the script
            var command = String.Format("\"{0}\" -cs \"{1}\" \"{2}\" \"{3}\" \"{4}\" \"{5}\" \"{6}\" \"{7}\" \"{8}\" \"{9}\" \"{10}\" ",
                rebolPath,
                scriptPath,
                Base64Service.Base64Encode(gmiPath),
                Base64Service.Base64Encode(outPath),
                Base64Service.Base64Encode(uri),
                Base64Service.Base64Encode(theme),
                Base64Service.Base64Encode(identiconUri.AbsoluteUri),
                Base64Service.Base64Encode(fabricUri.AbsoluteUri),
                Base64Service.Base64Encode(siteIdentity.GetId()),
                Base64Service.Base64Encode(siteIdentity.GetSiteId()),
                Base64Service.Base64Encode((showWebHeader ? "true" : "false"))

                );

            var execProcess = new ExecuteProcess();

            var result = execProcess.ExecuteCommand(command);

            return (result);
        }
    }


}
