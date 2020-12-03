/* --------------------------------------------------------------------------
 * Copyrights
 *
 * Portions created by or assigned to Cursive Systems, Inc. are
 * Copyright (c) 2002-2008 Cursive Systems, Inc.  All Rights Reserved.  Contact
 * information for Cursive Systems, Inc. is available at
 * http://www.cursive.net/.
 *
 * License
 *
 * Jabber-Net can be used under either JOSL or the GPL.
 * See LICENSE.txt for details.
 * --------------------------------------------------------------------------*/
using System;

using NUnit.Framework;

using bedrock.util;
using jabber.protocol;
using fact = jabber.protocol.stream.Factory;

namespace test.jabber.protocol.stream
{
    /// <summary>
    /// Summary description for StreamFactoryTest.
    /// </summary>
    [SVN(@"$Id: FactoryTest.cs 579 2008-02-13 21:29:33Z hildjj $")]
    [TestFixture]
    public class StreamFactoryTest
    {
        [Test] public void Test_Create()
        {
            ElementFactory pf = new ElementFactory();
            pf.AddType(new fact());
        }
    }
}
