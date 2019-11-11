﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using NitroSharp.NsScript.Primitives;

namespace NitroSharp.NsScript.VM
{
    internal sealed class BuiltInFunctionDispatcher
    {
        private readonly BuiltInFunctions _impl;
        private ConstantValue? _result;

        public BuiltInFunctionDispatcher(BuiltInFunctions functionsImpl)
        {
            _impl = functionsImpl;
        }

        private void SetResult(in ConstantValue value)
        {
            Debug.Assert(_result == null);
            _result = value;
        }

        public ConstantValue? Dispatch(BuiltInFunction function, ReadOnlySpan<ConstantValue> cvs)
        {
            var args = new ArgConsumer(cvs);
            switch (function)
            {
                case BuiltInFunction.CreateChoice:
                    CreateChoice(ref args);
                    break;
                case BuiltInFunction.SetAlias:
                    SetAlias(ref args);
                    break;
                case BuiltInFunction.Request:
                    Request(ref args);
                    break;
                case BuiltInFunction.Delete:
                    Delete(ref args);
                    break;
                case BuiltInFunction.CreateProcess:
                    CreateProcess(ref args);
                    break;
                case BuiltInFunction.Wait:
                    Wait(ref args);
                    break;
                case BuiltInFunction.WaitKey:
                    WaitKey(ref args);
                    break;

                case BuiltInFunction.CreateColor:
                    CreateColor(ref args);
                    break;
                case BuiltInFunction.LoadImage:
                    LoadImage(ref args);
                    break;
                case BuiltInFunction.CreateTexture:
                    CreateTexture(ref args);
                    break;
                case BuiltInFunction.CreateClipTexture:
                    CreateClipTexture(ref args);
                    break;
                case BuiltInFunction.DrawTransition:
                    DrawTransition(ref args);
                    break;
                case BuiltInFunction.SetShade:
                    SetShade(ref args);
                    break;
                case BuiltInFunction.SetTone:
                    SetTone(ref args);
                    break;

                case BuiltInFunction.Fade:
                    Fade(ref args);
                    break;
                case BuiltInFunction.Move:
                    Move(ref args);
                    break;
                case BuiltInFunction.Zoom:
                    Zoom(ref args);
                    break;

                case BuiltInFunction.CreateWindow:
                    CreateWindow(ref args);
                    break;
                case BuiltInFunction.CreateText:
                    CreateText(ref args);
                    break;
                case BuiltInFunction.LoadText:
                    LoadText(ref args);
                    break;
                case BuiltInFunction.WaitText:
                    WaitText();
                    break;

                case BuiltInFunction.CreateSound:
                    CreateSound(ref args);
                    break;
                case BuiltInFunction.SetVolume:
                    SetVolume(ref args);
                    break;
                case BuiltInFunction.SetLoop:
                    SetLoop(ref args);
                    break;
                case BuiltInFunction.SetLoopPoint:
                    SetLoopPoint(ref args);
                    break;

                case BuiltInFunction.DurationTime:
                    DurationTime(ref args);
                    break;
                case BuiltInFunction.PassageTime:
                    PassageTime(ref args);
                    break;
                case BuiltInFunction.RemainTime:
                    RemainTime(ref args);
                    break;
                case BuiltInFunction.ImageHorizon:
                    ImageHorizon(ref args);
                    break;
                case BuiltInFunction.ImageVertical:
                    ImageVertical(ref args);
                    break;
                case BuiltInFunction.Random:
                    Random(ref args);
                    break;
                case BuiltInFunction.SoundAmplitude:
                    SoundAmplitude(ref args);
                    break;
                case BuiltInFunction.Platform:
                    Platform();
                    break;
                case BuiltInFunction.ModuleFileName:
                    ModuleFileName();
                    break;
                case BuiltInFunction.String:
                    String(ref args);
                    break;
                case BuiltInFunction.Integer:
                    Integer(ref args);
                    break;
                case BuiltInFunction.Time:
                    Time();
                    break;
                case BuiltInFunction.ScrollbarValue:
                    ScrollbarValue(ref args);
                    break;
            }

            ConstantValue? result = _result;
            _result = null;
            return result;
        }

        private void SetTone(ref ArgConsumer args)
        {
            string entityQuery = args.TakeEntityQuery();
            if (args.TakeConstant() != BuiltInConstant.Monochrome)
            {
                throw new Exception();
            }

            _impl.Grayscale(entityQuery);
        }

        private void SetShade(ref ArgConsumer args)
        {
            _impl.BoxBlur(
                args.TakeEntityQuery(),
                nbPasses: args.TakeConstant() switch
                {
                    BuiltInConstant.Heavy => 16u,
                    BuiltInConstant.Medium => 8u,
                    _ => throw new Exception()
                }
            );
        }

        private void CreateText(ref ArgConsumer args)
        {
            _impl.CreateTextBlock(
                args.TakeEntityName(),
               priority: args.TakeInt(),
               x: args.TakeCoordinate(),
               y: args.TakeCoordinate(),
               width: args.TakeDimension(),
               height: args.TakeDimension(),
               pxmlText: args.TakeString()
            );
        }

        private void ScrollbarValue(ref ArgConsumer args)
        {
            string scrollbarEntity = args.TakeEntityName();
            SetResult(ConstantValue.Float(_impl.GetScrollbarValue(scrollbarEntity)));
        }

        private void Integer(ref ArgConsumer args)
        {
            ConstantValue value = args.TakeOpt(ConstantValue.Null);
            switch (value.Type)
            {
                case BuiltInType.Integer:
                    break;
                case BuiltInType.Float:
                    value = ConstantValue.Integer((int)value.AsFloat()!.Value);
                    break;
                    //default:
                    //    UnexpectedArgType<ConstantValue>(0, BuiltInType.Integer, value.Type);
                    //    break;
            }

            SetResult(value);
        }

        private void CreateWindow(ref ArgConsumer args)
        {
            _impl.CreateDialogueBox(
                args.TakeEntityName(),
                priority: args.TakeInt(),
                x: args.TakeCoordinate(),
                y: args.TakeCoordinate(),
                width: args.TakeInt(),
                height: args.TakeInt()
            );
        }

        private void WaitText()
        {
            _impl.WaitText(string.Empty, default);
        }

        private void LoadText(ref ArgConsumer args)
        {
            string subroutineName = args.TakeString();
            string boxName = args.TakeString();
            string textName = args.TakeString();
            int maxWidth = args.TakeInt();
            int maxHeight = args.TakeInt();
            int letterSpacing = args.TakeInt();
            int lineSpacing = args.TakeInt();

            NsxModule module = _impl.Interpreter.CurrentThread!.CallFrameStack.Peek(1).Module;
            int subroutineIdx = module.LookupSubroutineIndex(subroutineName);
            ref readonly SubroutineRuntimeInformation srti = ref module.GetSubroutineRuntimeInformation(subroutineIdx);
            int blockIndex = srti.LookupDialogueBlockIndex(textName);
            int offset = module.GetSubroutine(subroutineIdx).DialogueBlockOffsets[blockIndex];

            var token = new DialogueBlockToken(textName, boxName, module, subroutineIdx, offset);
            _impl.LoadText(token, maxWidth, maxHeight, letterSpacing, lineSpacing);
        }

        private void SetVolume(ref ArgConsumer args)
        {
            _impl.SetVolume(
                args.TakeEntityName(),
                duration: args.TakeTimeSpan(),
                volume: args.TakeRational()
            );
        }

        private void CreateSound(ref ArgConsumer args)
        {
            _impl.LoadAudio(
                args.TakeEntityName(),
                kind: args.TakeAudioKind(),
                fileName: args.TakeString()
            );
        }

        private void WaitKey(ref ArgConsumer args)
        {
            if (args.Count > 0)
            {
                TimeSpan timeout = args.TakeTimeSpan();
                _impl.WaitForInput(timeout);
            }
            else
            {
                _impl.WaitForInput();
            }
        }

        private void Wait(ref ArgConsumer args)
        {
            _impl.Delay(args.TakeTimeSpan());
        }

        private void SetLoopPoint(ref ArgConsumer args)
        {
            _impl.SetLoopRegion(
                args.TakeEntityName(),
                loopStart: args.TakeTimeSpan(),
                loopEnd: args.TakeTimeSpan()
            );
        }

        private void SetLoop(ref ArgConsumer args)
        {
            _impl.ToggleLooping(
                args.TakeEntityName(),
                looping: args.TakeBool()
            );
        }

        private void Zoom(ref ArgConsumer args)
        {
            string entityQuery = args.TakeEntityQuery();
            TimeSpan duration = args.TakeTimeSpan();
            _impl.Zoom(
                entityQuery,
                duration,
                dstScaleX: args.TakeRational(),
                dstScaleY: args.TakeRational(),
                easingFunction: args.TakeEasingFunction(),
                args.TakeAnimDelay(duration)
            );
        }

        private void Move(ref ArgConsumer args)
        {
            string entityQuery = args.TakeEntityQuery();
            TimeSpan duration = args.TakeTimeSpan();
            _impl.Move(
                entityQuery,
                duration,
                dstX: args.TakeCoordinate(),
                dstY: args.TakeCoordinate(),
                easingFunction: args.TakeEasingFunction(),
                delay: args.TakeAnimDelay(duration)
            );
        }

        private void Fade(ref ArgConsumer args)
        {
            string entityQuery = args.TakeEntityQuery();
            TimeSpan duration = args.TakeTimeSpan();
            _impl.Fade(
                entityQuery,
                duration,
                dstOpacity: args.TakeRational(),
                easingFunction: args.TakeEasingFunction(),
                args.TakeAnimDelay(duration)
            );
        }

        private void DrawTransition(ref ArgConsumer args)
        {
            string entityName = args.TakeEntityName();
            TimeSpan duration = args.TakeTimeSpan();
            _impl.DrawTransition(
                entityName,
                duration,
                initialOpacity: args.TakeRational(),
                finalOpacity: args.TakeRational(),
                feather: args.TakeRational(),
                args.TakeEasingFunction(),
                maskFileName: args.TakeString(),
                delay: args.TakeAnimDelay(duration)
            );
        }

        private void SetAlias(ref ArgConsumer args)
        {
            _impl.SetAlias(
                args.TakeEntityName(),
                alias: args.TakeString()
            );
        }

        private void Request(ref ArgConsumer args)
        {
            _impl.Request(args.TakeEntityQuery(), args.TakeEntityAction());
        }

        private void Delete(ref ArgConsumer args)
        {
            _impl.DestroyEntities(args.TakeEntityQuery());
        }

        private void CreateProcess(ref ArgConsumer args)
        {
            string name = args.TakeString();
            args.Skip();
            args.Skip();
            args.Skip();
            string target = args.TakeString();
            _impl.CreateThread(name, target);
        }

        private void CreateChoice(ref ArgConsumer args)
        {
            _impl.CreateChoice(args.TakeEntityName());
        }

        private void CreateColor(ref ArgConsumer args)
        {
            static NsColor takeColor(ref ArgConsumer args)
            {
                // Special case:
                // CreateColor("back05", 100, 0, 0, 1280, 720, null, "Black");
                //                                             ^^^^
                if (args.Count == 8)
                {
                    args.Skip();
                }

                return args.TakeColor();
            }

            _impl.CreateRectangle(
                args.TakeEntityName(),
                priority: args.TakeInt(),
                x: args.TakeCoordinate(),
                y: args.TakeCoordinate(),
                width: args.TakeInt(),
                height: args.TakeInt(),
                color: takeColor(ref args)
            );
        }

        private void LoadImage(ref ArgConsumer args)
        {
            _impl.LoadImage(
                args.TakeEntityName(),
                fileName: args.TakeString()
            );
        }

        private void CreateTexture(ref ArgConsumer args)
        {
            string entityName = args.TakeEntityName();
            int priority = args.TakeInt();
            NsCoordinate x = args.TakeCoordinate();
            NsCoordinate y = args.TakeCoordinate();

            ConstantValue arg4 = args.TakeOpt(ConstantValue.Null);
            string fileOrEntityName = arg4.Type switch
            {
                BuiltInType.String => ArgConsumer.EntityQuery(arg4.AsString()!),
                BuiltInType.BuiltInConstant => arg4.ConvertToString(),
                _ => args.UnexpectedType<string>(arg4.Type)
            };

            _impl.CreateSprite(entityName, priority, x, y, fileOrEntityName);
        }

        private void CreateClipTexture(ref ArgConsumer args)
        {
            _impl.CreateSpriteEx(
                args.TakeEntityName(),
                priority: args.TakeInt(),
                x1: args.TakeCoordinate(),
                y1: args.TakeCoordinate(),
                x2: args.TakeCoordinate(),
                y2: args.TakeCoordinate(),
                width: args.TakeInt(),
                height: args.TakeInt(),
                args.TakeEntityQuery()
            );
        }

        private void Time()
        {
            SetResult(ConstantValue.Integer(0));
        }

        private void String(ref ArgConsumer args)
        {
            string format = args.TakeString();
            var list = new List<object>();
            foreach (ref readonly ConstantValue arg in args.AsSpan(1))
            {
                int? num = arg.AsInteger();
                if (num != null)
                {
                    list.Add(num.Value);
                }
                else
                {
                    list.Add(arg.ConvertToString());
                }
            }

            SetResult(_impl.FormatString(format, list.ToArray()));
        }

        private void ModuleFileName()
        {
            SetResult(ConstantValue.String(_impl.GetCurrentModuleName()));
        }

        private void Platform()
        {
            SetResult(ConstantValue.Integer(_impl.GetPlatformId()));
        }

        private void SoundAmplitude(ref ArgConsumer args)
        {
            string characterName = args.TakeString();
            SetResult(ConstantValue.Integer(_impl.GetSoundAmplitude(characterName)));
        }

        private void Random(ref ArgConsumer args)
        {
            int max = args.TakeInt();
            SetResult(ConstantValue.Integer(_impl.GenerateRandomNumber(max)));
        }

        private void ImageHorizon(ref ArgConsumer args)
        {
            string entityName = args.TakeEntityQuery();
            SetResult(ConstantValue.Integer(_impl.GetWidth(entityName)));
        }

        private void ImageVertical(ref ArgConsumer args)
        {
            string entityName = args.TakeEntityQuery();
            SetResult(ConstantValue.Integer(_impl.GetHeight(entityName)));
        }

        private void RemainTime(ref ArgConsumer args)
        {
            string entityName = args.TakeEntityQuery();
            SetResult(ConstantValue.Integer(_impl.GetTimeRemaining(entityName)));
        }

        private void PassageTime(ref ArgConsumer args)
        {
            string entityName = args.TakeEntityQuery();
            SetResult(ConstantValue.Integer(_impl.GetTimeElapsed(entityName)));
        }

        private void DurationTime(ref ArgConsumer args)
        {
            string entityName = args.TakeEntityQuery();
            SetResult(ConstantValue.Integer(_impl.GetSoundDuration(entityName)));
        }

        private void Fail()
        {
            throw new NotImplementedException();
        }

        private void AssertEq(ref ArgConsumer args)
        {
            throw new NotImplementedException();
        }

        private void Assert(ref ArgConsumer args)
        {
        }

        private void Log(ref ArgConsumer args)
        {
            throw new NotImplementedException();
        }

        private ref struct ArgConsumer
        {
            private readonly ReadOnlySpan<ConstantValue> _args;
            private int _pos;

            public ArgConsumer(ReadOnlySpan<ConstantValue> args)
            {
                _args = args;
                _pos = 0;
            }

            public static string EntityQuery(string rawEntityQuery)
            {
                return rawEntityQuery.StartsWith("@")
                    ? rawEntityQuery.Substring(1)
                    : rawEntityQuery;
            }

            private static TimeSpan Time(int ms) => TimeSpan.FromMilliseconds(ms);

            public int Count => _args.Length;

            public ReadOnlySpan<ConstantValue> AsSpan(int start) => _args.Slice(start);
            public void Skip() => _pos++;

            public ConstantValue TakeOpt(in ConstantValue defaultValue)
            {
                return _pos < _args.Length
                    ? _args[_pos++]
                    : defaultValue;
            }

            public string TakeEntityName() => EntityQuery(TakeString());
            public string TakeEntityQuery() => EntityQuery(TakeString());

            public string TakeString()
            {
                ConstantValue arg = TakeOpt(ConstantValue.EmptyString);
                return arg.AsString()!;
            }

            public int TakeInt()
            {
                ConstantValue arg = TakeOpt(ConstantValue.Integer(0));
                return arg.AsInteger()!.Value;
            }

            public TimeSpan TakeTimeSpan()
            {
                ConstantValue val = TakeOpt(ConstantValue.Integer(0));
                int num = val.Type switch
                {
                    BuiltInType.Integer => val.AsInteger()!.Value,
                    BuiltInType.String => int.Parse(val.AsString()!),
                    _ => UnexpectedType<int>(val.Type)
                };
                return Time(num);
            }

            public bool TakeBool()
            {
                ConstantValue arg = TakeOpt(ConstantValue.False);
                return arg.AsBool()!.Value;
            }

            public NsColor TakeColor()
            {
                ConstantValue val = TakeOpt(ConstantValue.Null);
                return val.Type switch
                {
                    BuiltInType.String => NsColor.FromString(val.AsString()!),
                    BuiltInType.Integer => NsColor.FromRgb(val.AsInteger()!.Value),
                    BuiltInType.BuiltInConstant => NsColor.FromConstant(val.AsBuiltInConstant()!.Value),
                    _ => UnexpectedType<NsColor>(val.Type)
                };
            }

            public NsCoordinate TakeCoordinate()
            {
                ConstantValue val = TakeOpt(ConstantValue.Integer(0));
                return val.Type switch
                {
                    BuiltInType.Integer
                        => NsCoordinate.WithValue(val.AsInteger()!.Value, NsCoordinateOrigin.Zero, 0),
                    BuiltInType.DeltaInteger
                        => NsCoordinate.WithValue(val.AsDelta()!.Value, NsCoordinateOrigin.CurrentValue, 0),
                    BuiltInType.BuiltInConstant
                        => NsCoordinate.FromConstant(val.AsBuiltInConstant()!.Value),

                    _ => UnexpectedType<NsCoordinate>(val.Type)
                };
            }

            public NsDimension TakeDimension()
            {
                ConstantValue val = TakeOpt(ConstantValue.Integer(0));
                return val.Type switch
                {
                    BuiltInType.Integer
                        => NsDimension.WithValue(val.AsInteger()!.Value),
                    BuiltInType.BuiltInConstant
                        => NsDimension.FromConstant(val.AsBuiltInConstant()!.Value),
                    _ => UnexpectedType<NsDimension>(val.Type)
                };
            }

            public NsEasingFunction TakeEasingFunction()
            {
                ConstantValue val = TakeOpt(ConstantValue.Null);
                if (val.Type == BuiltInType.BuiltInConstant)
                {
                    return EnumConversions.ToEasingFunction(val.AsBuiltInConstant()!.Value);
                }
                else if (val.Type == BuiltInType.Null)
                {
                    return NsEasingFunction.None;
                }

                return UnexpectedType<NsEasingFunction>(val.Type);
            }

            public NsAudioKind TakeAudioKind()
            {
                ConstantValue val = TakeOpt(ConstantValue.Null);
                return val.Type == BuiltInType.BuiltInConstant
                    ? EnumConversions.ToAudioKind(val.AsBuiltInConstant()!.Value)
                    : UnexpectedType<NsAudioKind>(val.Type);
            }

            public NsEntityAction TakeEntityAction()
            {
                ConstantValue val = TakeOpt(ConstantValue.Null);
                return val.Type == BuiltInType.BuiltInConstant
                    ? EnumConversions.ToEntityAction(val.AsBuiltInConstant()!.Value)
                    : UnexpectedType<NsEntityAction>(val.Type);
            }

            public NsRational TakeRational(float denominator = 1000.0f)
            {
                return new NsRational(TakeInt(), denominator);
            }

            public TimeSpan TakeAnimDelay(TimeSpan animDuration)
            {
                int delayArg = TakeInt();
                return delayArg == 1 ? animDuration : TimeSpan.FromMilliseconds(delayArg);
            }

            public BuiltInConstant TakeConstant()
            {
                ConstantValue val = TakeOpt(ConstantValue.Null);
                return val.Type == BuiltInType.BuiltInConstant
                    ? val.AsBuiltInConstant()!.Value
                    : UnexpectedType<BuiltInConstant>(val.Type);
            }

            public T UnexpectedType<T>(BuiltInType type)
                => throw new NsxCallDispatchException(_pos - 1, type);
        }
    }
}
