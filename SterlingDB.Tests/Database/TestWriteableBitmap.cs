//using System;
//using System.Collections.Generic;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using Microsoft.Phone.Testing;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using SterlingDB.Database;

//namespace SterlingDB.Test.Database
//{
//    public class ImageClass
//    {
//        public int Id { get; set; }
//        public WriteableBitmap BitMap { get; set; }
//    }

//    public class ImageDatabase : BaseDatabaseInstance
//    {
//        /// <summary>
//        ///     The name of the database instance
//        /// </summary>
//        public override string Name
//        {
//            get { return "Images"; }
//        }

//        /// <summary>
//        ///     Method called from the constructor to register tables
//        /// </summary>
//        /// <returns>The list of tables for the database</returns>
//        protected override List<ITableDefinition> RegisterTables()
//        {
//            return new List<ITableDefinition>
//                           {
//                               CreateTableDefinition<ImageClass, int>(n => n.Id)
//                           };
//        }
//    }

//    [Tag("Image")]
//    [Tag("Database")]
//    
//    public class TestWriteableBitmap : SilverlightTest
//    {
//        private readonly SterlingEngine _engine;
//        private ISterlingDatabaseInstance _databaseInstance;

//        
//        public void TestInit()
//        {            
//            _engine = new SterlingEngine();
//            _engine.Activate();
//            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<ImageDatabase>();
//            _databaseInstance.PurgeAsync().Wait();
//        }

//        
//        public override void Cleanup()
//        {
//            _databaseInstance.PurgeAsync().Wait();
//            _engine.Dispose();
//            _databaseInstance = null;            
//        }

//        [Asynchronous]
//        [Fact]
//        public void TestImageSaveAndRestore()
//        {
//            var grid = new Grid();
//            grid.ColumnDefinitions.Add(new ColumnDefinition { Width=new GridLength(1,GridUnitType.Star)});
//            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
//            var image = new Image
//                            {
//                                Source =
//                                    new BitmapImage(
//                                    new Uri("/SterlingDB.Test.Helpers;component/Sterling-Final-Small.png",
//                                            UriKind.Relative))
//                            };
//            image.SetValue(Grid.ColumnProperty, 0);

//            var loaded = false;

//            image.Loaded += (o, e) => loaded = true;

//            grid.Children.Add(image);

//            TestPanel.Children.Add(grid);

//            EnqueueConditional(() => loaded);

//            EnqueueCallback(() =>
//                                {
//                                    var expected = new ImageClass
//                                                       {
//                                                           Id = 1,
//                                                           BitMap = new WriteableBitmap(image, new TranslateTransform())
//                                                       };

//                                    var key = _databaseInstance.Save(expected);
//                                    var actual = _databaseInstance.Load<ImageClass>(key);
//                                    Assert.NotNull(actual, "Test failed: ");
//                                    Assert.NotNull(actual.BitMap, "Test failed: actual bitmap is null.");
//                                    Assert.Equal(expected.BitMap.PixelHeight, actual.BitMap.PixelHeight,
//                                                    "Test failed: height mismatch.");
//                                    Assert.Equal(expected.BitMap.PixelWidth, actual.BitMap.PixelWidth,
//                                                    "Test failed: width mismatch.");
//                                    loaded = false;
//                                    var image2 = new Image
//                                                     {
//                                                         Source = actual.BitMap
//                                                     };
//                                    image2.SetValue(Grid.ColumnProperty, 1);
//                                    image2.Loaded += (o, e) => loaded = true;
//                                    grid.Children.Add(image2);
//                                });

//            EnqueueConditional(() => loaded);
//            EnqueueDelay(500);
//            EnqueueTestComplete();
//        }
//    }
//}