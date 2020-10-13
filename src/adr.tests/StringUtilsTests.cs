using adr.Utils;
using Xunit;

namespace tests
{
    public class StringUtilsTests
    {
        [Fact]
        public void Diacritics_Must_Be_Folded()
        {
            var test01 = "crème brûlée";
            var test02 = "ÁÂÄÅÇÉÍÎÓÖØÚÜÞàáâãäåæçèéêëìíîïðñóôöøúüāăčĐęğıŁłńŌōřŞşšźžșțệủ";
            var test03 = "⑦ⓧ＊⁈～»Ꝡ";

            Assert.Equal("creme brulee", StringUtils.FoldToASCII(test01));
            Assert.Equal("AAAACEIIOOOUUTHaaaaaaaeceeeeiiiidnoooouuaacDegiLlnOorSsszzsteu", StringUtils.FoldToASCII(test02));
            Assert.Equal("7x*?!~\"VY", StringUtils.FoldToASCII(test03));
        }
    }
}
