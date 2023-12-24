using System.Linq;
using Backend.API.Permissions;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Backend.API.Tests.Unit.Permissions;

[TestClass]
[TestSubject(typeof(Permission))]
public class PermissionTest
{
    [TestMethod]
    public void TestPermissionConstructor()
    {
        var permission = new Permission("key1", "title1");
        Assert.AreEqual("key1", permission.Value);
        Assert.AreEqual("title1", permission.Title);
        Assert.AreEqual("key1", permission.Key);
        Assert.IsFalse(permission.IsChecked);
        Assert.IsNotNull(permission.Nodes);
    }

    [TestMethod]
    public void TestAddNode()
    {
        var permission = new Permission("key1", "title1");
        var node = permission.AddNode("key2", "title2");
        Assert.AreEqual("key2", node.Key);
        Assert.AreEqual("title2", node.Title);
        Assert.AreEqual("key1.key2", node.Value);
        Assert.IsTrue(permission.Nodes.Contains(node));
    }

    [TestMethod]
    public void TestPermissionListConstructor()
    {
        var permissionList = new PermissionList();
        Assert.IsNotNull(permissionList.Permissions);
        Assert.IsInstanceOfType(permissionList.Permissions.First(), typeof(AdministrativePermission));
    }

    [TestMethod]
    public void TestGetPermissionByKey()
    {
        var permissionList = new PermissionList();
        var permission = permissionList.GetPermissionByKey(AdministrativePermission.AdministrativeManageUser);
        Assert.IsNotNull(permission);
        Assert.AreEqual(AdministrativePermission.AdministrativeManageUser, permission.Value);
    }

    [TestMethod]
    public void TestSetPermissionEnable()
    {
        var permissionList = new PermissionList();
        permissionList.SetPermissionEnable(AdministrativePermission.AdministrativeManageUser);
        var permission = permissionList.GetPermissionByKey(AdministrativePermission.AdministrativeManageUser);
        Assert.IsTrue(permission.IsChecked);
    }

    [TestMethod]
    public void TestAdministrativePermissionConstructor()
    {
        var adminPermission = new AdministrativePermission();
        Assert.AreEqual("Administrative", adminPermission.Key);
        Assert.AreEqual("Manage Users", adminPermission.Title);
        Assert.AreEqual("Manage Users", adminPermission.Nodes.First().Title);
        Assert.AreEqual("View Users", adminPermission.Nodes.Skip(1).First().Title);
    }
}

[TestClass]
[TestSubject(typeof(PermissionList))]
public class PermissionListTest
{
    private PermissionList _permissionList;

    [TestInitialize]
    public void Setup()
    {
        _permissionList = new PermissionList();
    }

    [TestMethod]
    public void Permissons_StartsWithAdminPermission()
    {
        Assert.IsTrue(_permissionList.Permissions.First() is AdministrativePermission);
    }

    [TestMethod]
    public void GetPermissionByKey_ForExistingKey()
    {
        var permission = _permissionList.GetPermissionByKey(AdministrativePermission.AdministrativeManageUser);

        Assert.IsNotNull(permission);
        Assert.IsInstanceOfType(permission, typeof(Permission));
    }

    [TestMethod]
    public void GetPermissionByKey_ForNonExistingKey()
    {
        var permission = _permissionList.GetPermissionByKey("Non.Existing.Key");

        Assert.IsNull(permission);
    }

    [TestMethod]
    public void SetPermissionEnable_ForExistingKey()
    {
        _permissionList.SetPermissionEnable(AdministrativePermission.AdministrativeManageUser);

        var permission = _permissionList.GetPermissionByKey(AdministrativePermission.AdministrativeManageUser);
        Assert.IsTrue(permission.IsChecked);
    }

    [TestMethod]
    public void SetPermissionEnable_ForNonExistingKey()
    {
        _permissionList.SetPermissionEnable("Non.Existing.Key");

        var permission = _permissionList.GetPermissionByKey("Non.Existing.Key");
        Assert.IsNull(permission);
    }
}