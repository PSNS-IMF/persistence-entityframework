using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Psns.Common.Test.BehaviorDrivenDevelopment;
using Psns.Common.Persistence.EntityFramework;
using Psns.Common.Persistence.Definitions;

using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.ComponentModel.DataAnnotations;

using Moq;

namespace EntityFramework.UnitTests
{
    public class TestEntity : IIdentifiable
    {
        public int Id { get; set; }
    }

    public class ModelContext : Context
    {
        public virtual DbSet<TestEntity> TestEntities { get; set; }
        public virtual DbSet<KeyedNonId> KeyedNonIds { get; set; }
    }

    public class BadContext : DbContext { }

    public class WhenWorkingWithTheRepository : BehaviorDrivenDevelopmentCaseTemplate
    {
        protected Repository<TestEntity> Repository;
        protected Mock<ModelContext> MockContext;
        protected Mock<DbSet<TestEntity>> MockTestEntitySet;
        protected Mock<DbSet<KeyedNonId>> MockKeyedNonIdSet;

        public override void Arrange()
        {
            base.Arrange();

            var data = new List<TestEntity>
            {
                new TestEntity { Id = 1 },
                new TestEntity { Id = 2 },
            }.AsQueryable();

            MockTestEntitySet = new Mock<DbSet<TestEntity>>();
            MockTestEntitySet.As<IQueryable<TestEntity>>().Setup(m => m.Provider).Returns(data.Provider);
            MockTestEntitySet.As<IQueryable<TestEntity>>().Setup(m => m.Expression).Returns(data.Expression);
            MockTestEntitySet.As<IQueryable<TestEntity>>().Setup(m => m.ElementType).Returns(data.ElementType);
            MockTestEntitySet.As<IQueryable<TestEntity>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            MockContext = new Mock<ModelContext>();

            MockContext.Setup(c => c.Set<TestEntity>()).Returns(MockTestEntitySet.Object);

            Repository = new Repository<TestEntity>(MockContext.Object);
        }
    }

    [TestClass]
    public class AndConstructingWithADbContextThatDoesntImplementIContext
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ThenAnInvalidOperationExceptionShouldBeThrown()
        {
            var repository = new Repository<TestEntity>(new BadContext());
            Assert.Fail();
        }
    }

    [TestClass]
    public class AndGettingAll : WhenWorkingWithTheRepository
    {
        IEnumerable<TestEntity> _all;

        public override void Arrange()
        {
            base.Arrange();

            MockContext.Setup(c => c.Set<TestEntity>().AsNoTracking()).Returns(MockTestEntitySet.Object);
        }

        public override void Act()
        {
            base.Act();

            _all = Repository.All();
        }

        [TestMethod]
        public void ThenAllEntitiesShouldBeReturned()
        {
            Assert.AreEqual(2, _all.Count());
            Assert.AreEqual(1, _all.ElementAt(0).Id);
        }
    }

    [TestClass]
    public class AndDeleting : WhenWorkingWithTheRepository
    {
        TestEntity _deleted;

        public override void Arrange()
        {
            base.Arrange();

            MockTestEntitySet.Setup(s => s.Remove(It.Is<TestEntity>(e => e.Id == 1))).Returns(new TestEntity { Id = 1 });
        }

        public override void Act()
        {
            base.Act();

            _deleted = Repository.Delete(new TestEntity { Id = 1 });
        } 

        [TestMethod]
        public void ThenTheRemovedObjectShouldBeReturned()
        {
            Assert.AreEqual(1, _deleted.Id);
        }
    }

    [TestClass]
    public class AndFinding : WhenWorkingWithTheRepository
    {
        TestEntity _found;

        public override void Arrange()
        {
            base.Arrange();

            MockTestEntitySet.Setup(s => s.Find(It.IsAny<object[]>())).Returns(new TestEntity { Id = 1 });
        }

        public override void Act()
        {
            base.Act();

            _found = Repository.Find(1);
        }

        [TestMethod]
        public void ThenTheFoundEntityShouldBeReturned()
        {
            Assert.AreEqual(1, _found.Id);
        }
    }

    [TestClass]
    public class AndFindingWithPredicate : WhenWorkingWithTheRepository
    {
        IEnumerable<TestEntity> _found;

        public override void Act()
        {
            base.Act();

            _found = Repository.Find(e => e.Id == 1);
        }

        [TestMethod]
        public void ThenTheFoundEntityShouldBeReturned()
        {
            Assert.AreEqual(1, _found.Single().Id);
        }
    }

    [TestClass]
    public class AndCreating : WhenWorkingWithTheRepository
    {
        TestEntity _created;

        public override void Arrange()
        {
            base.Arrange();

            MockTestEntitySet.Setup(s => s.Add(It.IsAny<TestEntity>())).Returns(new TestEntity { Id = 1 });
        }

        public override void Act()
        {
            base.Act();

            _created = Repository.Create(new TestEntity { Id = 1 });
        }

        [TestMethod]
        public void ThenTheCreatedObjectShouldBeReturned()
        {
            Assert.AreEqual(1, _created.Id);
        }
    }

    [TestClass]
    public class AndUpdating : WhenWorkingWithTheRepository
    {
        TestEntity _updated;

        public override void Arrange()
        {
            base.Arrange();

            MockTestEntitySet.Setup(s => s.Find(It.IsAny<object[]>())).Returns(new TestEntity { Id = 1 });
        }

        public override void Act()
        {
            base.Act();

            _updated = Repository.Update(new TestEntity { Id = 1 }, null);
        }

        [TestMethod]
        public void ThenTheUpdatedObjectShouldBeReturned()
        {
            Assert.AreEqual(1, _updated.Id);
            MockContext.Verify(c => c.SetModified(It.IsAny<object>(), It.IsAny<object>()), Times.Once());
        }
    }

    public class NonKeyed : IIdentifiable
    {
        public NonKeyed()
        {
            TestEntities = new List<TestEntity>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<TestEntity> TestEntities { get; set; }
    }

    public class KeyedNonId : IIdentifiable
    {
        public KeyedNonId()
        {
            TestEntities = new List<TestEntity>();
        }

        public int Id { get; set; }

        [Key]
        public string Name { get; set; }

        public ICollection<TestEntity> TestEntities { get; set; }
    }

    [TestClass]
    public class AndUpdatingWithAKeyedNonIdEntity : WhenWorkingWithTheRepository
    {
        KeyedNonId _old;
        KeyedNonId _updated;
        Repository<KeyedNonId> _repository;

        public override void Arrange()
        {
            base.Arrange();

            var data = new List<KeyedNonId>
            {
                new KeyedNonId { Id = 1, Name = "1" },
                new KeyedNonId { Id = 2, Name = "2" },
            }.AsQueryable();

            MockKeyedNonIdSet = new Mock<DbSet<KeyedNonId>>();
            MockKeyedNonIdSet.As<IQueryable<KeyedNonId>>().Setup(m => m.Provider).Returns(data.Provider);
            MockKeyedNonIdSet.As<IQueryable<KeyedNonId>>().Setup(m => m.Expression).Returns(data.Expression);
            MockKeyedNonIdSet.As<IQueryable<KeyedNonId>>().Setup(m => m.ElementType).Returns(data.ElementType);
            MockKeyedNonIdSet.As<IQueryable<KeyedNonId>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            MockKeyedNonIdSet.Setup(s => s.Find(It.IsAny<object[]>())).Returns(() =>
                {
                    _old = new KeyedNonId
                   {
                       Id = 1,
                       Name = "updated",
                       TestEntities = new List<TestEntity>
                        {
                            new TestEntity { Id = 2 },
                            new TestEntity { Id = 3 },
                            new TestEntity { Id = 1 }
                        }
                   };

                    return _old;
                });

            MockContext = new Mock<ModelContext>();

            MockContext.Setup(c => c.Set<KeyedNonId>()).Returns(MockKeyedNonIdSet.Object);

            _repository = new Repository<KeyedNonId>(MockContext.Object);

            RelationshipLoaderAdapter.LoadFunction = (DbContext context, object entity, string[] includes) => { return; };
        }

        public override void Act()
        {
            base.Act();

            _updated = _repository.Update(new KeyedNonId 
            { 
                Name = "updated", 
                TestEntities = new List<TestEntity> 
                { 
                    new TestEntity { Id = 1 },
                    new TestEntity { Id = 4 }
                } 
            }, "TestEntities");
        }

        [TestMethod]
        public void ThenTheUpdatedObjectShouldBeReturned()
        {
            Assert.AreEqual("updated", _updated.Name);
            Assert.AreEqual(2, _old.TestEntities.Count);
            Assert.AreEqual(1, _old.TestEntities.ElementAt(0).Id);
            Assert.AreEqual(4, _old.TestEntities.ElementAt(1).Id);

            Assert.AreEqual(2, _updated.TestEntities.Count);
            Assert.AreEqual(1, _updated.TestEntities.ElementAt(0).Id);
            Assert.AreEqual(4, _updated.TestEntities.ElementAt(1).Id);

            MockContext.Verify(c => c.SetModified(It.Is<object>(o => (o as KeyedNonId).Id == 1),
                It.Is<object>(o => (o as KeyedNonId).Id == 0)), Times.Once());
        }
    }

    [TestClass]
    public class AndSavingChanges : WhenWorkingWithTheRepository
    {
        public override void Act()
        {
            base.Act();

            Repository.SaveChanges();
        }

        [TestMethod]
        public void ThenSaveChangesShouldBeCalled()
        {
            MockContext.Verify(c => c.SaveChanges(), Times.Once());
        }
    }

    public class RepositoryHelper : Repository<TestEntity>
    {
        public RepositoryHelper(DbContext context) : base(context) { }

        public DbContext Context { get { return _dbContext; } }
    }

    [TestClass]
    public class AndDisposing : WhenWorkingWithTheRepository
    {
        public override void Arrange()
        {
            base.Arrange();

            Repository = new RepositoryHelper(MockContext.Object);
        }

        public override void Act()
        {
            base.Act();

            Repository.Dispose();
        }

        [TestMethod]
        public void ThenTheDbContextShouldSetToNull()
        {
            Assert.IsNull((Repository as RepositoryHelper).Context);
        }
    }
}
