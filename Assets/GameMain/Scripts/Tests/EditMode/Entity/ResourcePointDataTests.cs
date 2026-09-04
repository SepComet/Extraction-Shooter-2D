using System;
using System.Collections.Generic;
using NUnit.Framework;
using SepCore.Entity;
using UnityEngine;

namespace SepCore.Tests
{
    [TestFixture]
    public class ResourcePointDataTests
    {
        [Test]
        public void Constructor_ValidArguments_InitializesPropertiesCorrectly()
        {
            Vector3 position = new Vector3(10f, 20f, 0f);
            Quaternion rotation = Quaternion.Euler(0f, 0f, 90f);
            List<int> itemIds = new List<int> { 5001, 5002, 5003 };

            ResourcePointData data = new ResourcePointData(
                entityId: 100,
                assetName: "ResourcePoint1",
                position: position,
                resourcePointId: 1,
                itemIds: itemIds,
                rotation: rotation
            );

            Assert.AreEqual(100, data.Id);
            Assert.AreEqual("ResourcePoint1", data.AssetName);
            Assert.AreEqual(position, data.Position);
            Assert.AreEqual(rotation, data.Rotation);
            Assert.AreEqual(1, data.ResourcePointId);
            Assert.AreEqual(3, data.ItemCount);
            CollectionAssert.AreEqual(new[] { 5001, 5002, 5003 }, data.ItemIds);
        }

        [Test]
        public void Constructor_NullRotation_DefaultsToIdentity()
        {
            ResourcePointData data = new ResourcePointData(
                entityId: 1,
                assetName: "ResourcePoint1",
                position: Vector3.one,
                resourcePointId: 2,
                itemIds: new List<int>()
            );

            Assert.AreEqual(Quaternion.identity, data.Rotation);
            Assert.AreEqual(0, data.ItemCount);
        }

        [Test]
        public void Constructor_NullItemIds_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new ResourcePointData(
                    entityId: 1,
                    assetName: "ResourcePoint1",
                    position: Vector3.zero,
                    resourcePointId: 1,
                    itemIds: null
                );
            });
        }

        [Test]
        public void ItemIds_IsDefensiveCopy_ExternalModificationsDoNotAffect()
        {
            List<int> sourceList = new List<int> { 101, 102 };
            ResourcePointData data = new ResourcePointData(
                entityId: 1,
                assetName: "ResourcePoint1",
                position: Vector3.zero,
                resourcePointId: 1,
                itemIds: sourceList
            );

            sourceList.Add(103);

            Assert.AreEqual(2, data.ItemCount);
            CollectionAssert.AreEqual(new[] { 101, 102 }, data.ItemIds);
        }
    }
}
