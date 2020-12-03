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
 *
 * xpnet is a deriviative of James Clark's XP.  See copying.txt for more info.
 * --------------------------------------------------------------------------*/
namespace xpnet
{
    using bedrock.util;

    /**
     * Represents a position in an entity.
     * A position can be modified by <code>Encoding.movePosition</code>.
     * @see Encoding#movePosition
     * @version $Revision: 579 $ $Date: 2008-02-13 14:29:33 -0700 (Wed, 13 Feb 2008) $
     */
    ///<summary>
    /// Position of an entry in a table.
    ///</summary>
    [SVN(@"$Id: Position.cs 579 2008-02-13 21:29:33Z hildjj $")]
    public class Position : System.ICloneable
    {
        private int lineNumber;
        private int columnNumber;

        /**
         * Creates a position for the start of an entity: the line number is
         * 1 and the column number is 0.
         */
        public Position()
        {
            lineNumber = 1;
            columnNumber = 0;
        }

        /**
         * Returns the line number.
         * The first line number is 1.
         */
        public int LineNumber
        {
            get {return lineNumber;}
            set {lineNumber = value;}
        }

        /**
         * Returns the column number.
         * The first column number is 0.
         * A tab character is not treated specially.
         */
        public int ColumnNumber
        {
            get { return columnNumber; }
            set { columnNumber = value; }
        }

        /**
         * Returns a copy of this position.
         */
        public object Clone()
        {
            throw new System.NotImplementedException();
        }
    }
}
