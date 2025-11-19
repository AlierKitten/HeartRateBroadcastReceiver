using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
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
                MinLimit = 0,
                MaxLimit = 220,
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

        // 保持最多60个数据点（大约1分钟的数据）
        while (_heartRateData.Count > 60)
        {
            _heartRateData.RemoveAt(0);
        }
    }

    public void ClearHeartRateData()
    {
        _heartRateData.Clear();
        _startTime = DateTime.Now;
    }
}