using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace HeartRateBroadcastReceiver.ViewModels.Pages;

public partial class DataPageViewModel : ObservableObject
{
    private readonly ObservableCollection<ObservablePoint> _heartRateData;
    private DateTime _startTime;

    public DataPageViewModel()
    {
        _heartRateData = new ObservableCollection<ObservablePoint>();
        _startTime = DateTime.Now;

        Series = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = _heartRateData,
                Name = "心率 (BPM)",
                Fill = null,
                Stroke = new SolidColorPaint(new SKColor(255, 82, 161), 3),
                GeometryFill = new SolidColorPaint(new SKColor(255, 255, 255)),
                GeometryStroke = new SolidColorPaint(new SKColor(0, 0, 0), 2),
                GeometrySize = 10,
                DataPadding = new LiveChartsCore.Drawing.LvcPoint(0, 1),
                LineSmoothness = 0.5,
                IsHoverable = true
            }
        };

        XAxes = new[]
        {
            new Axis
            {
                Labeler = value => TimeSpan.FromSeconds(value).ToString(@"mm\:ss"),
                UnitWidth = 1,
                MinStep = 1,
                SeparatorsPaint = new SolidColorPaint(new SKColor(220, 220, 220)),
                TextSize = 14,
                LabelsRotation = 0
            }
        };

        YAxes = new[]
        {
            new Axis
            {
                SeparatorsPaint = new SolidColorPaint(new SKColor(220, 220, 220)),
                TextSize = 14
            }
        };
    }

    public ISeries[] Series { get; set; }

    public Axis[] XAxes { get; set; }

    public Axis[] YAxes { get; set; }

    public void AddHeartRateData(int heartRate)
    {
        // 添加新的数据点，使用相对于开始时间的秒数
        var elapsed = (DateTime.Now - _startTime).TotalSeconds;
        _heartRateData.Add(new ObservablePoint(elapsed, heartRate));

        // 保持最多60个数据点
        while (_heartRateData.Count > 60)
        {
            _heartRateData.RemoveAt(0);
        }

        // 动态调整Y轴范围
        UpdateYAxisRange();
    }

    public void ClearHeartRateData()
    {
        _heartRateData.Clear();
        _startTime = DateTime.Now;

        // 重置Y轴范围到默认状态
        YAxes[0].MinLimit = 0;
        YAxes[0].MaxLimit = 220;
    }

    private void UpdateYAxisRange()
    {
        if (_heartRateData.Count == 0) return;

        // 获取当前所有数据点的Y值
        var yValues = _heartRateData
            .Where(p => p.Y.HasValue)
            .Select(p => p.Y.Value)
            .ToList();

        if (yValues.Count == 0) return;

        // 计算最小值和最大值
        double min = yValues.Min();
        double max = yValues.Max();

        // 添加一些边距，使图形不紧贴边缘
        double margin = (max - min) * 0.1; // 10%的边距
        if (margin == 0) margin = 5; // 如果所有值相同，添加固定边距

        // 确保最小值不低于0
        YAxes[0].MinLimit = Math.Max(0, min - margin);
        YAxes[0].MaxLimit = max + margin;
    }
}