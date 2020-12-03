using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwoRatChat.Main.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TwoRatChat.Main.Model.Tests {
    [TestClass()]
    public class CustomUsersTests {
        [TestMethod()]
        public void SaveLoadTest() {
            CustomUsers cu = new CustomUsers();
            cu.Add( new CustomUserItem() {
                Nickname = "oxlamon",
                Level = 1,
                Blacklisted = false,
                BlacklistPhrase = "none",
                Expirience = 42,
                WelcomePhrase = "great",
                Source = "oxlamon"
            } );

            MemoryStream ms = new MemoryStream();
            cu.save( ms );

            ms = new MemoryStream( ms.ToArray() );

            CustomUsers lu = CustomUsers.load( ms );

            Assert.IsNotNull( lu );
            Assert.AreNotSame( lu, cu );

            var user = lu.FirstOrDefault();

            Assert.IsNotNull( user );

            Assert.AreEqual<int>( 1, user.Level );
            Assert.AreEqual<long>( 42, user.Expirience );
            Assert.AreEqual<string>( "great", user.WelcomePhrase );
            Assert.AreEqual<string>( "oxlamon", user.Source );
            Assert.AreEqual<string>( "oxlamon", user.Nickname );
        }
    }
}