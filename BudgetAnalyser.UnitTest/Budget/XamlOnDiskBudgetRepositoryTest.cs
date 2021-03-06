﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xaml;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.UnitTest.Helper;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rees.TestUtilities;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class XamlOnDiskBudgetRepositoryTest
    {
        [TestMethod]
        public async Task CreateNewShouldPopulateFileName()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.WriteToDiskMock = (f, d) => { };
            SetPrivateBudgetCollection(subject);
            var filename = "FooBar.xml";
            BudgetCollection collection = await subject.CreateNewAndSaveAsync(filename);
            Assert.AreEqual(filename, collection.FileName);
        }

        [TestMethod]
        public async Task CreateNewShouldReturnCollectionWithOneBudgetInIt()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.WriteToDiskMock = (f, d) => { };
            SetPrivateBudgetCollection(subject);
            var filename = "FooBar.xml";
            BudgetCollection collection = await subject.CreateNewAndSaveAsync(filename);

            Assert.IsTrue(collection.Count == 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateNewShouldThrowGivenEmptyFileName()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            await subject.CreateNewAndSaveAsync(string.Empty);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateNewShouldThrowGivenNullFileName()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            await subject.CreateNewAndSaveAsync(null);
            Assert.Fail();
        }

        [TestMethod]
        public async Task CreateNewShouldWriteToDisk()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            var writeToDiskCalled = false;
            subject.WriteToDiskMock = (f, d) => { writeToDiskCalled = true; };
            SetPrivateBudgetCollection(subject);
            var filename = "FooBar.xml";
            await subject.CreateNewAndSaveAsync(filename);
            Assert.IsTrue(writeToDiskCalled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullDomainMapper()
        {
            new XamlOnDiskBudgetRepository(
                new BucketBucketRepoAlwaysFind(),
                new BasicMapperFake<BudgetCollection, BudgetCollectionDto>(),
                null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullDtoMapper()
        {
            new XamlOnDiskBudgetRepository(
                new BucketBucketRepoAlwaysFind(),
                null,
                new BasicMapperFake<BudgetCollectionDto, BudgetCollection>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowWhenBucketRepositoryIsNull()
        {
            new XamlOnDiskBudgetRepository(
                null,
                new BasicMapperFake<BudgetCollection, BudgetCollectionDto>(),
                new BasicMapperFake<BudgetCollectionDto, BudgetCollection>());
            Assert.Fail();
        }

        [TestMethod]
        public async Task LoadShouldCallInitialiseOnTheBucketRepository()
        {
            var mockBucketRepository = new Mock<IBudgetBucketRepository>();
            mockBucketRepository.Setup(m => m.Initialise(null));

            var toDomainMapper = new Mock<BasicMapper<BudgetCollectionDto, BudgetCollection>>();
            var subject = new XamlOnDiskBudgetRepositoryTestHarness(
                mockBucketRepository.Object,
                new BasicMapperFake<BudgetCollection, BudgetCollectionDto>(),
                toDomainMapper.Object);
            toDomainMapper.Setup(m => m.Map(It.IsAny<BudgetCollectionDto>())).Returns(BudgetModelTestData.CreateCollectionWith1And2);
            subject.FileExistsMock = f => true;

            subject.LoadFromDiskMock = OnLoadFromDiskMock;
            await subject.LoadAsync(TestDataConstants.BudgetCollectionTestDataFileName);

            mockBucketRepository.Verify();
        }

        [TestMethod]
        public async Task LoadShouldReturnACollectionAndSetFileName()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = OnLoadFromDiskMock;
            BudgetCollection collection = await subject.LoadAsync(TestDataConstants.BudgetCollectionTestDataFileName);

            Assert.AreEqual(TestDataConstants.BudgetCollectionTestDataFileName, collection.FileName);
        }

        [TestMethod]
        [ExpectedException(typeof(DataFormatException))]
        public async Task LoadShouldThrowIfAnyOtherExceptionIsThrownAndWrapItIntoFileFormatException()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = f => { throw new Exception(); };
            await subject.LoadAsync("SmellyPoo.xml");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(DataFormatException))]
        public async Task LoadShouldThrowIfDeserialisedObjectIsNotBudgetCollection()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = f => new object();
            await subject.LoadAsync("SmellyPoo.xml");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task LoadShouldThrowIfFileDoesntExist()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.FileExistsMock = f => false;
            await subject.LoadAsync("SmellyPoo.xml");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(DataFormatException))]
        public async Task LoadShouldThrowIfFileFormatIsInvalid()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = f => { throw new XamlObjectWriterException(); };
            await subject.LoadAsync("SmellyPoo.xml");

            Assert.Fail();
        }

        [TestMethod]
        public async Task MustBeAbleToLoadDemoBudgetFile()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = OnLoadFromDiskMock;
            BudgetCollection collection = await subject.LoadAsync(TestDataConstants.DemoBudgetFileName);

            Assert.AreEqual(TestDataConstants.DemoBudgetFileName, collection.FileName);
            Assert.AreEqual(1, collection.Count);
        }

        [TestMethod]
        public async Task MustBeAbleToLoadEmptyBudgetFile()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = OnLoadFromDiskMock;
            BudgetCollection collection = await subject.LoadAsync(TestDataConstants.EmptyBudgetFileName);

            Assert.AreEqual(TestDataConstants.EmptyBudgetFileName, collection.FileName);
            Assert.AreEqual(1, collection.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SaveShouldThrowIfLoadHasntBeenCalled()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            await subject.SaveAsync();
            Assert.Fail();
        }

        [TestMethod]
        public async Task SaveShouldWriteToDisk()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            var writeToDiskCalled = false;
            subject.WriteToDiskMock = (filename, data) => { writeToDiskCalled = true; };
            SetPrivateBudgetCollection(subject);

            await subject.SaveAsync();

            Assert.IsTrue(writeToDiskCalled);
        }

        [TestInitialize]
        public void TestInitialise()
        {
        }

        private XamlOnDiskBudgetRepositoryTestHarness Arrange(IBudgetBucketRepository bucketRepo = null)
        {
            if (bucketRepo == null)
            {
                bucketRepo = new InMemoryBudgetBucketRepository(new DtoToBudgetBucketMapper());
            }

            return new XamlOnDiskBudgetRepositoryTestHarness(bucketRepo);
        }

        private static BudgetCollectionDto OnLoadFromDiskMock(string f)
        {
            return EmbeddedResourceHelper.ExtractXaml<BudgetCollectionDto>(f);
        }

        private static void SetPrivateBudgetCollection(XamlOnDiskBudgetRepositoryTestHarness subject)
        {
            PrivateAccessor.SetField<XamlOnDiskBudgetRepository>(subject, "currentBudgetCollection", BudgetModelTestData.CreateCollectionWith1And2());
        }
    }
}