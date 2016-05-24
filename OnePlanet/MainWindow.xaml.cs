using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using NoToolkitDxLib;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using Matrix = SharpDX.Matrix;
using Point = System.Windows.Point;
using Plane = NoToolkitDxLib.Plane;
using MyMesh = NoToolkitDxLib.MyMesh<SharpDX.Vector4>;

namespace OnePlanet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private DxManager _manager;
        private Matrix _view;
        private Matrix _proj;


        private Stopwatch _clock;
        private ConstantBuffer _cbuf;
        private Buffer _constantBuffer;


        private MyMesh _sphere;
        private MyMesh _sun;
        private MyMesh _plane;



        private double _distSum;


        public MainWindow()
        {
            InitializeComponent();

            Loaded += (sender, args) =>
             Catch(() =>
             {
                 _manager = new DxManager(SharpDxElement);
                 Prepare();
             });


            SharpDxElement.OnResized +=
                (source, e) =>
                    Catch(
                        () =>

                            _proj =
                                Matrix.PerspectiveFovRH(
                                    MathUtil.PiOverFour,
                                    (float)SharpDxElement.ActualWidth / (float)SharpDxElement.ActualHeight,
                                    0.1f,
                                    1000.0f)

                    );


            new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(15),
                IsEnabled = true,
            }.Tick += (sender, args) => Draw();



            Closing += (sender, args) => Clean();

            Point popPos = default(Point);

            _distSum = 40;

            SharpDxElement.MouseDown += (s, a) => popPos = a.GetPosition(this);

            SharpDxElement.MouseMove += (s, a) =>
            {
                if (a.LeftButton == MouseButtonState.Released) return;
                Point position = a.GetPosition(this);

                float viewRotationAngleY = -(float)(position.Y - popPos.Y) / 80f;
                float viewRotationAngleZ = -(float)(position.X - popPos.X) / 80f;

                _viewRotation = Quaternion.RotationAxis(Vector3.UnitY, viewRotationAngleY) * Quaternion.RotationAxis(Vector3.UnitZ, viewRotationAngleZ) * _viewRotation;

                popPos = position;

            };

            SharpDxElement.MouseWheel += (sender, args) => _distSum = Math.Max(3f, _distSum - args.Delta / 10f);


        }

        private void Clean()
        {
            _sphere.Dispose();
            _sun.Dispose();
            _plane.Dispose();
            Utilities.Dispose(ref _manager);
        }


        private void Catch(Action todo)
        {
            try
            {
                todo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        private void Prepare()
        {

            var vertexShaderByteCode = ShaderBytecode.CompileFromFile("shaders.fx", "VS", "vs_4_0");
            var vertexShader = new VertexShader(_manager.Device, vertexShaderByteCode);

            var pixelShaderByteCode = ShaderBytecode.CompileFromFile("shaders.fx", "PS", "ps_4_0");
            var pixelShader = new PixelShader(_manager.Device, pixelShaderByteCode);

            var signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

            var layoutElements = new[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 0, 0),
                new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 16, 0),
                new InputElement("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 32, 0),
            };

            var layout = new InputLayout(_manager.Device, signature, layoutElements);
            int stride = layoutElements.Length;

            _sphere = GenSphere(stride);
            _sun = GenSun(stride);
            _plane = GenPlane(stride);

            _constantBuffer = new Buffer(_manager.Device, Utilities.SizeOf<ConstantBuffer>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            var context = _manager.Device.ImmediateContext;
            context.InputAssembler.InputLayout = layout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            context.VertexShader.SetConstantBuffer(0, _constantBuffer);
            context.VertexShader.Set(vertexShader);
            context.PixelShader.SetConstantBuffer(0, _constantBuffer);
            context.PixelShader.Set(pixelShader);

            _proj = Matrix.Identity;

            _clock = Stopwatch.StartNew();

            Vector3 lightDirection = new Vector3(1, 1, 1f);
            lightDirection.Normalize();

            _cbuf = new ConstantBuffer
            {
                LightDir = new Vector4(lightDirection, 1),
                Light = 1
            };

            _prevGameSec = (float)_clock.Elapsed.TotalSeconds;
            _viewRotation = Quaternion.RotationAxis(Vector3.UnitY, 0) * Quaternion.RotationAxis(Vector3.UnitZ, 0);

        }

        
        private MyMesh GenSphere(int stride)
        {
            var sphere = new Sphere(.5f, 10);
            var sphereColor = Color.CornflowerBlue;
            var sphereVert =
                sphere.Vertices.SelectMany(el => new[]
                {
                    new Vector4(el.Position, 1f),
                    sphereColor.ToVector4(),
                    new Vector4(el.Normal, 0f)
                }).ToArray();
            return new MyMesh(_manager.Device, sphereVert, sphere.Indices, stride);
        }


        private MyMesh GenSun(int stride)
        {
            var sun = new Sphere(1.5f, 10);
            var sunColor = Color.Yellow;
            var sunVert =
                sun.Vertices.SelectMany(el => new[]
                {
                    new Vector4(el.Position, 1f),
                    sunColor.ToVector4(),
                    new Vector4(el.Normal, 0f)
                }).ToArray();

            return new MyMesh(_manager.Device, sunVert, sun.Indices, stride);
        }


        private MyMesh GenPlane(int stride)
        {
            var plane = new Plane(60f, 60f, 15);
            var planeIndices = plane.Indices.Concat(plane.Indices.Reverse()).ToArray();

            var planeVert =
                plane.Vertices.SelectMany(el =>
                {
                    float fx = el.Position.X*1f;
                    float fy = el.Position.Y*1f;
                    double arg = Math.Sqrt(fx*fx + fy*fy);
                    var val = arg > 1e-9 ? (float) (Math.Sin(arg)/arg) : 1;
                    float col = Math.Max(0, .8f + val*2);

                    return new[]
                    {
                        new Vector4(el.Position, 1f),
                        new Vector4(col/2, col/2, col, 1f),
                        new Vector4(el.Normal, 0f)
                    };
                }).ToArray();

            return new MyMesh(_manager.Device, planeVert, planeIndices, stride);
        }


        private Quaternion _viewRotation;

        private double _prevGameSec;

        private Vector3 _velocity = new Vector3(
            Properties.Settings.Default.VelocityX,
            Properties.Settings.Default.VelocityY,
            Properties.Settings.Default.VelocityZ);

        private Vector3 _position = new Vector3(
            Properties.Settings.Default.PositionX,
            Properties.Settings.Default.PositionY,
            Properties.Settings.Default.PositionZ);


        private readonly float _scale = Properties.Settings.Default.Scale;
        private readonly float _timeDelta = Properties.Settings.Default.TimeDelta;


        private void Draw()
        {
            _manager.Clear(Color.Black);

            var context = _manager.Device.ImmediateContext;

            var eyePosition = Vector3.Transform(new Vector3(0, (float)_distSum / 3f, 0), _viewRotation);

            _view = Matrix.LookAtRH(eyePosition, new Vector3(0, 0, 0), Vector3.UnitZ);

            var time = (float)_clock.Elapsed.TotalSeconds;

            float dt = _timeDelta;
            int stepsTodo = (int)Math.Floor((time - _prevGameSec) / dt);

            while (stepsTodo-- > 0)
            {
                float r = Math.Max(0.05f, _position.Length());
                Vector3 acc = _position * (-1f / r / r / r);
                _velocity = _velocity + acc * dt;
                _position = _position + _velocity * dt;
            }

            SetWorld(context, Matrix.Translation(_position * _scale) * Matrix.Translation(0, 0, 3));
            _sphere.Draw();

            SetWorld(context, Matrix.Translation(0f, 0f, -4f));
            _plane.Draw();

            SetWorld(context, Matrix.Translation(0, 0, 3));
            _sun.Draw();

            _manager.Present();

            _prevGameSec = time;
        }


        private void SetWorld(DeviceContext context, Matrix world)
        {
            var viewProj = _view * _proj;
            _cbuf.World = world;
            _cbuf.WorldViewProj = _cbuf.World * viewProj;
            context.UpdateSubresource(ref _cbuf, _constantBuffer);
        }

    }
}
