/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (ExceptionExtensions.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
 * 
 * dotEntity is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 
 * dotEntity is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * GNU Affero General Public License for more details.
 
 * You should have received a copy of the GNU Affero General Public License
 * along with dotEntity.If not, see<http://www.gnu.org/licenses/>.

 * You can release yourself from the requirements of the license by purchasing
 * a commercial license.Buying such a license is mandatory as soon as you
 * develop commercial software involving the dotEntity software without
 * disclosing the source code of your own applications.
 * To know more about our commercial license email us at support@roastedbytes.com or
 * visit http://dotentity.net/legal/commercial
 */
using System;
using System.Diagnostics;
using System.Linq;

namespace DotEntity.Extensions
{
    public static class ExceptionExtensions
    {
        public static string GetStackTraceWithoutSkippedMethods(this Exception e)
        {
#if NETSTANDARD15
            return e.StackTrace;
#else
            return string.Concat(
                new StackTrace(e, true)
                    .GetFrames()
                    .Where(frame => !frame.GetMethod().IsDefined(typeof(SkipFromStackTrace), true))
                    .Select(frame => new StackTrace(frame).ToString())
                    .ToArray());
#endif
        }
    }
}