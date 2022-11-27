using System;
using SloperMobile.Model.BucketsModel;
using Xamarin.Forms;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using System.Threading.Tasks;
using SloperMobile.Common.Helpers;
using System.Linq;
using System.Collections.Generic;
using SloperMobile.Model;
using SloperMobile.Common.Interfaces;

namespace SloperMobile
{
    public static class DataTemplateHelper
    {
        //public static async Task<DataTemplate> GetBucketsTemplateAsyncOld() 
        //{
        //    var bucketRepository = new Repository<BucketTable>();
        //    var cragRepository = new Repository<CragExtended>();

        //    var bucketItem = await bucketRepository.QueryAsync<BucketsModel>($"SELECT DISTINCT bucket_name as BucketName, hex_code as HexColor FROM T_BUCKET WHERE (grade_type_id IN(SELECT DISTINCT T_ROUTE.grade_type_id FROM T_SECTOR INNER JOIN T_ROUTE ON T_SECTOR.sector_id = T_ROUTE.sector_id WHERE(T_SECTOR.crag_id = {Settings.ActiveCrag}))) ORDER BY grade_bucket_group, grade_bucket_id");
        //    if (bucketItem == null || bucketItem.Count() == 0)
        //    {
        //        var gradeid = await cragRepository.ExecuteScalarAsync<string>($"SELECT route_grades FROM T_CRAG Where crag_id = {Settings.ActiveCrag}");
        //        if (!string.IsNullOrEmpty(gradeid))
        //        {
        //            var x = gradeid.Split(',');
        //            string myQuery = "SELECT DISTINCT bucket_name as BucketName, hex_code as HexColor FROM T_BUCKET WHERE (grade_type_id IN (" + gradeid + ")) ORDER BY grade_bucket_group, grade_bucket_id";
        //            bucketItem = await bucketRepository.QueryAsync<BucketsModel>(myQuery);
        //        }
        //    }

        //    var leg_buckets = bucketItem;
        //    if (leg_buckets != null)
        //    {
        //        int gc = await bucketRepository.ExecuteScalarAsync<int>("SELECT Count(grade_type_id) As BucketCount FROM T_BUCKET GROUP BY grade_type_id LIMIT 1");
        //        int gr = leg_buckets.Count() / gc;
        //        Grid grdLegend = new Grid();
        //        for (var i = 0; i < gr; i++)
        //        {
        //            grdLegend.RowDefinitions?.Add(new RowDefinition { Height = GridLength.Auto });
        //        }

        //        for (var i = 0; i < gc; i++)
        //        {
        //            grdLegend.ColumnDefinitions?.Add(new ColumnDefinition { Width = GridLength.Star });
        //        }

        //        var batches = leg_buckets.Select((x, i) => new { x, i }).GroupBy(p => (p.i / gc), p => p.x);

        //        int r = 0;
        //        foreach (var row in batches)
        //        {
        //            int c = 0;
        //            foreach (var item in row)
        //            {
        //                grdLegend.Children.Add(new Label { Text = item.BucketName, HorizontalTextAlignment = TextAlignment.Center, FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)), TextColor = Color.FromHex(item.HexColor) }, c, r);
        //                c++;
        //            }
        //            r++;
        //        }

        //        return new DataTemplate(() =>
        //        {
        //            return grdLegend;
        //        });

        //    }
        //    return null;
        //}

        public static async Task<DataTemplate> GetBucketsTemplateAsync(int cragId, List<RouteTable> routes = null, IExceptionSynchronizationManager exceptionManager = null)
        {
            try
            {
                var bucketRepository = new Repository<BucketTable>();
                var cragRepository = new Repository<CragExtended>();

                List<int> routeGrades = null;
                IEnumerable<BucketTable> dbBuckets = null;

                if (routes != null)
                {
                    routeGrades = routes.Select(r => r.grade_type_id).Distinct().ToList();
                }
                else
                {
                    var currentCrag = await cragRepository.GetAsync(cragId);
                    routeGrades = currentCrag.route_grades.Split(',').Select(s => int.Parse(s)).ToList();
                }
                dbBuckets = (await bucketRepository.GetAsync(b => routeGrades.Contains(b.grade_type_id))).Distinct(new BucketComparer());

                var groupedBuckets = dbBuckets.OrderBy(b => b.grade_type_id).GroupBy(b => b.grade_type_id).ToList();

                var grdLegend = new Grid();
                grdLegend.RowSpacing = 0;
                grdLegend.Margin = 0;
                grdLegend.Padding = 0;

                foreach (var row in groupedBuckets)
                    grdLegend.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                foreach (var col in groupedBuckets[0])
                    grdLegend.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                for (int r = 0; r < groupedBuckets.Count; r++)
                {
                    for (int c = 0; c < groupedBuckets[r].Count(); c++)
                    {
                        var bucket = groupedBuckets[r].ElementAt(c);
                        grdLegend.Children.Add(new Label { Text = bucket.bucket_name, HorizontalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center, FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)), TextColor = Color.FromHex(bucket.hex_code) }, c, r);
                    }
                }

                return new DataTemplate(() =>
                {
                    return grdLegend;
                });
            }
            catch (Exception ex)
            {
                exceptionManager.LogException(new ExceptionTable {
                    Method = nameof(GetBucketsTemplateAsync),
                    Page = nameof(DataTemplateHelper),
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message,
                    Data = $"cragId: {cragId};routes: {routes};"
                });
                return null;
            }
        }

        public static async Task<List<BucketsSegmentModel>> GetBucketsSourceAsync(int cragId, List<RouteTable> routes = null, IExceptionSynchronizationManager exceptionManager = null)
        {
            try
            {
                var bucketRepository = new Repository<BucketTable>();
                var cragRepository = new Repository<CragExtended>();

                List<int> routeGrades = null;
                IEnumerable<BucketTable> dbBuckets = null;

                if (routes != null)
                {
                    routeGrades = routes.Select(r => r.grade_type_id).Distinct().ToList();
                }
                else
                {
                    var currentCrag = await cragRepository.GetAsync(cragId);
                    routeGrades = currentCrag.route_grades.Split(',').Select(s => int.Parse(s)).ToList();
                }
                dbBuckets = (await bucketRepository.GetAsync(b => routeGrades.Contains(b.grade_type_id))).Distinct(new BucketComparer());

                var groupedBuckets = dbBuckets.GroupBy(x => x.grade_bucket_id).Select(b => new BucketsSegmentModel
                {
                    Buckets = b.Select(x => x).ToList()
                }).ToList();

                return groupedBuckets;
            }
            catch (Exception ex)
            {
                exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(GetBucketsSourceAsync),
                    Page = nameof(DataTemplateHelper),
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message,
                    Data = $"cragId: {cragId};routes: {routes};"
                });
                return null;
            }
        }
    }
}
