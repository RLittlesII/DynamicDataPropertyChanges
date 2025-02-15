using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Bogus;
using DynamicData;
using DynamicData.Binding;
using FluentAssertions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ServerSideTests;

public sealed class Test1
{
    [Fact]
    public void Given_WhenDepartmentsPropertyChanged_ThenDepartmentsPropertyNamedChanged()
    {
        // Given
        var result = string.Empty;
        Store sut = FakeStore.Initialize();

        using var _ = sut.Changed.Subscribe(x => result = x.PropertyName);

        // When
        sut.Departments = new Departments();

        // Then
        result.Should().Be(nameof(Store.Departments));
    }

    [Fact]
    public void Given_WhenDepartmentsItem1PropertyChanged_ThenDepartmentsPropertyNamedChanged()
    {
        // Given
        var result = string.Empty;
        Store sut = FakeStore.Initialize();

        using var _ = sut.Departments.Changed.Subscribe(x => result = x.PropertyName);

        // When
        sut.Departments[0].Item1 = new Item();

        // Then
        result.Should().Be(nameof(sut.Departments));
    }

    [Fact]
    public void Given_WhenDepartmentsItem1PropertyChanged_ThenItem1PropertyNamedChanged()
    {
        // Given
        var result = string.Empty;
        Store sut = FakeStore.Initialize();

        using var _ = sut.Departments[0].Item1.Changed.Subscribe(x => result = x.PropertyName);

        // When
        sut.Departments[0].Item1 = new Item();

        // Then
        result.Should().Be(nameof(Department.Item1));
    }

    [Fact]
    public void Given_When_Then()
    {
        // Given
        var result = string.Empty;
        Store sut = FakeStore.Initialize();

        using var _ = sut.Changed.Subscribe(x => result = x.PropertyName);

        // When
        sut.Departments[0].Item1.SoldAtLocations[0].Name = ThisIsOnlyATest;

        // Then
        result.Should().NotBeNullOrEmpty();
    }

    private const string ThisIsOnlyATest = "this is only a test";
}

public class Store : ReactiveObject
{
    public Store()
    {
        //I assume I need to do something here.....
    }

    public Departments Departments
    {
        get => _department;
        set => this.RaiseAndSetIfChanged(ref _department, value);
    }

    private Departments _department;
}

public class Departments : ReactiveObject, IObservableCollection<Department>
{
    event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
    {
        add => _departments.CollectionChanged += value;
        remove => _departments.CollectionChanged -= value;
    }

    IEnumerator<Department> IEnumerable<Department>.GetEnumerator() => _departments.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _departments.GetEnumerator();

    void ICollection<Department>.Add(Department item) => _departments.Add(item);

    void ICollection<Department>.Clear() => _departments.Clear();

    bool ICollection<Department>.Contains(Department item) => _departments.Contains(item);

    void ICollection<Department>.CopyTo(Department[] array, int arrayIndex) => _departments.CopyTo(array, arrayIndex);

    bool ICollection<Department>.Remove(Department item) => _departments.Remove(item);

    int ICollection<Department>.Count => _departments.Count;

    bool ICollection<Department>.IsReadOnly => false;

    int IList<Department>.IndexOf(Department item) => _departments.IndexOf(item);

    void IList<Department>.Insert(int index, Department item) => _departments.Insert(index, item);

    void IList<Department>.RemoveAt(int index) => _departments.RemoveAt(index);

    public Department this[int index]
    {
        get => _departments[index];
        set => _departments[index] = value;
    }

    IDisposable INotifyCollectionChangedSuspender.SuspendCount() => _departments.SuspendCount();

    IDisposable INotifyCollectionChangedSuspender.SuspendNotifications() => _departments.SuspendNotifications();

    void IObservableCollection<Department>.Load(IEnumerable<Department> items) => _departments.Load(items);

    void IObservableCollection<Department>.Move(int oldIndex, int newIndex) => _departments.Move(oldIndex, newIndex);

    private ObservableCollectionExtended<Department> _departments = new();
}

public class Department : ReactiveObject
{
    public int Id { get; set; }
    public string Name { get; set; }
    [Reactive] public Item Item1 { get; set; }
    [Reactive] public Item Item2 { get; set; }
    [Reactive] public Item Item3 { get; set; }
    [Reactive] public Item Item4 { get; set; }
}

public class Item : ReactiveObject
{
    [Reactive] public int Id { get; set; }
    [Reactive] public string Name { get; set; }
    [Reactive] public DateOnly CreatedDate { get; set; }
    [Reactive] public IObservableCollection<Location> SoldAtLocations { get; set; }
}

public class Location : ReactiveObject
{
    [Reactive] public int Id { get; set; }

    [Reactive] public string Name { get; set; }

    [Reactive] public string State { get; set; }
}

public class FakeStore
{
    public static Store Initialize()
    {
        var locationFaker = new Faker<Location>()
            .RuleFor(l => l.Id, f => f.IndexFaker + 1)
            .RuleFor(l => l.Name, f => f.Address.City())
            .RuleFor(l => l.State, f => f.Address.State());

        // Faker for Item
        var itemFaker = new Faker<Item>()
            .RuleFor(i => i.Id, f => f.IndexFaker + 1)
            .RuleFor(i => i.Name, f => f.Commerce.ProductName())
            .RuleFor(i => i.CreatedDate, f => DateOnly.FromDateTime(f.Date.Past()))
            .RuleFor(i => i.SoldAtLocations, f =>
            {
                var locations = new ObservableCollectionExtended<Location>();
                locations.AddRange(locationFaker.Generate(f.Random.Int(1, 5))); // Random number of locations
                return locations;
            });

        // Faker for Department
        var departmentFaker = new Faker<Department>()
            .RuleFor(d => d.Id, f => f.IndexFaker + 1)
            .RuleFor(d => d.Name, f => f.Commerce.Department())
            .RuleFor(d => d.Item1, f => itemFaker.Generate())
            .RuleFor(d => d.Item2, f => itemFaker.Generate())
            .RuleFor(d => d.Item3, f => itemFaker.Generate())
            .RuleFor(d => d.Item4, f => itemFaker.Generate());

        // Faker for StoreViewModel
        var storeViewModelFaker = new Faker<Store>()
            .RuleFor(s => s.Departments, f =>
            {
                var departments = new Departments();
                departments.AddRange(departmentFaker.Generate(f.Random.Int(2, 5))); // Random number of departments
                return departments;
            });

        // Generate the StoreViewModel with fake data
        var fakeStore = storeViewModelFaker.Generate();
        return fakeStore;
    }
}