using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockEF.Tests.Data;
using MockEF.Moq;
using System.Threading.Tasks;

namespace MockEF.Tests
{
    [TestClass]
    public class MoqMockEfDbContextTests
    {
        private MockEfDbContextTests _test = new MockEfDbContextTests();

        [TestMethod]
        public void Moq_SeededDataIsFound()
        {
            var builder = new ContextBuilder<IMyContext>();
            _test.SeededDataIsFound(builder);
        }

        [TestMethod]
        public async Task Moq_SeededDataIsFoundAsync()
        {
            var builder = new ContextBuilder<IMyContext>();
            await _test.SeededDataIsFoundAsync(builder);
        }

        [TestMethod]
        public void Moq_DynamicallyAddedData_StaysPersisted()
        {
            var builder = new ContextBuilder<IMyContext>();
            _test.NewDynamicallyAddedData_StaysPersistedMethod(builder);
        }

        [TestMethod]
        public void Moq_FindOnSeededData_ReturnsValue()
        {
            var builder = new ContextBuilder<IMyContext>();
            _test.FindOnSeededData_ReturnsValue(builder);
        }

        [TestMethod]
        public void Moq_FindOnSeededData_WithCompositeKey_ReturnsValue()
        {
            var builder = new ContextBuilder<IMyContext>();
            _test.FindOnSeededData_WithCompositeKey_ReturnsValue(builder);
        }

        [TestMethod]
        public void Moq_DeletedSeedData_StaysRemoved()
        {
            var builder = new ContextBuilder<IMyContext>();
            _test.DeletedSeedData_StaysRemoved(builder);
        }
    }
}
