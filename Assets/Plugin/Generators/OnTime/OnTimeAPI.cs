// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
using Newtonsoft.Json;

namespace OnTimeAPI
{

public class Auxtimer1
{
    [JsonConstructor]
    public Auxtimer1(
        [JsonProperty("duration")] int duration,
        [JsonProperty("current")] int current,
        [JsonProperty("playback")] string playback,
        [JsonProperty("direction")] string direction
    )
    {
        this.duration = duration;
        this.current = current;
        this.playback = playback;
        this.direction = direction;
    }

    [JsonProperty("duration")]
    public int duration { get; }

    [JsonProperty("current")]
    public int current { get; }

    [JsonProperty("playback")]
    public string playback { get; }

    [JsonProperty("direction")]
    public string direction { get; }
}

    public class CurrentBlock
    {
        [JsonConstructor]
        public CurrentBlock(
            [JsonProperty("block")] object block,
            [JsonProperty("startedAt")] int startedAt
        )
        {
            this.block = block;
            this.startedAt = startedAt;
        }

        [JsonProperty("block")]
        public object block { get; }

        [JsonProperty("startedAt")]
        public int startedAt { get; }
    }

    public class Custom
    {
        [JsonConstructor]
        public Custom(
            [JsonProperty("song")] string song,
            [JsonProperty("artist")] string artist
        )
        {
            this.song = song;
            this.artist = artist;
        }

        [JsonProperty("song")]
        public string song { get; }

        [JsonProperty("artist")]
        public string artist { get; }
    }

    public class EventNext
    {
        [JsonConstructor]
        public EventNext(
            [JsonProperty("type")] string type,
            [JsonProperty("id")] string id,
            [JsonProperty("cue")] string cue,
            [JsonProperty("title")] string title,
            [JsonProperty("note")] string note,
            [JsonProperty("endAction")] string endAction,
            [JsonProperty("timerType")] string timerType,
            [JsonProperty("countToEnd")] bool countToEnd,
            [JsonProperty("linkStart")] object linkStart,
            [JsonProperty("timeStrategy")] string timeStrategy,
            [JsonProperty("timeStart")] int timeStart,
            [JsonProperty("timeEnd")] int timeEnd,
            [JsonProperty("duration")] int duration,
            [JsonProperty("isPublic")] bool isPublic,
            [JsonProperty("skip")] bool skip,
            [JsonProperty("colour")] string colour,
            [JsonProperty("revision")] int revision,
            [JsonProperty("delay")] int delay,
            [JsonProperty("dayOffset")] int dayOffset,
            [JsonProperty("gap")] int gap,
            [JsonProperty("timeWarning")] int timeWarning,
            [JsonProperty("timeDanger")] int timeDanger,
            [JsonProperty("custom")] Custom custom
        )
        {
            this.type = type;
            this.id = id;
            this.cue = cue;
            this.title = title;
            this.note = note;
            this.endAction = endAction;
            this.timerType = timerType;
            this.countToEnd = countToEnd;
            this.linkStart = linkStart;
            this.timeStrategy = timeStrategy;
            this.timeStart = timeStart;
            this.timeEnd = timeEnd;
            this.duration = duration;
            this.isPublic = isPublic;
            this.skip = skip;
            this.colour = colour;
            this.revision = revision;
            this.delay = delay;
            this.dayOffset = dayOffset;
            this.gap = gap;
            this.timeWarning = timeWarning;
            this.timeDanger = timeDanger;
            this.custom = custom;
        }

        [JsonProperty("type")]
        public string type { get; }

        [JsonProperty("id")]
        public string id { get; }

        [JsonProperty("cue")]
        public string cue { get; }

        [JsonProperty("title")]
        public string title { get; }

        [JsonProperty("note")]
        public string note { get; }

        [JsonProperty("endAction")]
        public string endAction { get; }

        [JsonProperty("timerType")]
        public string timerType { get; }

        [JsonProperty("countToEnd")]
        public bool countToEnd { get; }

        [JsonProperty("linkStart")]
        public object linkStart { get; }

        [JsonProperty("timeStrategy")]
        public string timeStrategy { get; }

        [JsonProperty("timeStart")]
        public int timeStart { get; }

        [JsonProperty("timeEnd")]
        public int timeEnd { get; }

        [JsonProperty("duration")]
        public int duration { get; }

        [JsonProperty("isPublic")]
        public bool isPublic { get; }

        [JsonProperty("skip")]
        public bool skip { get; }

        [JsonProperty("colour")]
        public string colour { get; }

        [JsonProperty("revision")]
        public int revision { get; }

        [JsonProperty("delay")]
        public int delay { get; }

        [JsonProperty("dayOffset")]
        public int dayOffset { get; }

        [JsonProperty("gap")]
        public int gap { get; }

        [JsonProperty("timeWarning")]
        public int timeWarning { get; }

        [JsonProperty("timeDanger")]
        public int timeDanger { get; }

        [JsonProperty("custom")]
        public Custom custom { get; }
    }

    public class EventNow
    {
        [JsonConstructor]
        public EventNow(
            [JsonProperty("type")] string type,
            [JsonProperty("id")] string id,
            [JsonProperty("cue")] string cue,
            [JsonProperty("title")] string title,
            [JsonProperty("note")] string note,
            [JsonProperty("endAction")] string endAction,
            [JsonProperty("timerType")] string timerType,
            [JsonProperty("countToEnd")] bool countToEnd,
            [JsonProperty("linkStart")] object linkStart,
            [JsonProperty("timeStrategy")] string timeStrategy,
            [JsonProperty("timeStart")] int timeStart,
            [JsonProperty("timeEnd")] int timeEnd,
            [JsonProperty("duration")] int duration,
            [JsonProperty("isPublic")] bool isPublic,
            [JsonProperty("skip")] bool skip,
            [JsonProperty("colour")] string colour,
            [JsonProperty("revision")] int revision,
            [JsonProperty("delay")] int delay,
            [JsonProperty("dayOffset")] int dayOffset,
            [JsonProperty("gap")] int gap,
            [JsonProperty("timeWarning")] int timeWarning,
            [JsonProperty("timeDanger")] int timeDanger,
            [JsonProperty("custom")] Custom custom
        )
        {
            this.type = type;
            this.id = id;
            this.cue = cue;
            this.title = title;
            this.note = note;
            this.endAction = endAction;
            this.timerType = timerType;
            this.countToEnd = countToEnd;
            this.linkStart = linkStart;
            this.timeStrategy = timeStrategy;
            this.timeStart = timeStart;
            this.timeEnd = timeEnd;
            this.duration = duration;
            this.isPublic = isPublic;
            this.skip = skip;
            this.colour = colour;
            this.revision = revision;
            this.delay = delay;
            this.dayOffset = dayOffset;
            this.gap = gap;
            this.timeWarning = timeWarning;
            this.timeDanger = timeDanger;
            this.custom = custom;
        }

        [JsonProperty("type")]
        public string type { get; }

        [JsonProperty("id")]
        public string id { get; }

        [JsonProperty("cue")]
        public string cue { get; }

        [JsonProperty("title")]
        public string title { get; }

        [JsonProperty("note")]
        public string note { get; }

        [JsonProperty("endAction")]
        public string endAction { get; }

        [JsonProperty("timerType")]
        public string timerType { get; }

        [JsonProperty("countToEnd")]
        public bool countToEnd { get; }

        [JsonProperty("linkStart")]
        public object linkStart { get; }

        [JsonProperty("timeStrategy")]
        public string timeStrategy { get; }

        [JsonProperty("timeStart")]
        public int timeStart { get; }

        [JsonProperty("timeEnd")]
        public int timeEnd { get; }

        [JsonProperty("duration")]
        public int duration { get; }

        [JsonProperty("isPublic")]
        public bool isPublic { get; }

        [JsonProperty("skip")]
        public bool skip { get; }

        [JsonProperty("colour")]
        public string colour { get; }

        [JsonProperty("revision")]
        public int revision { get; }

        [JsonProperty("delay")]
        public int delay { get; }

        [JsonProperty("dayOffset")]
        public int dayOffset { get; }

        [JsonProperty("gap")]
        public int gap { get; }

        [JsonProperty("timeWarning")]
        public int timeWarning { get; }

        [JsonProperty("timeDanger")]
        public int timeDanger { get; }

        [JsonProperty("custom")]
        public Custom custom { get; }
    }

    public class Message
    {
        [JsonConstructor]
        public Message(
            [JsonProperty("timer")] Timer timer,
            [JsonProperty("external")] string external
        )
        {
            this.timer = timer;
            this.external = external;
        }

        [JsonProperty("timer")]
        public Timer timer { get; }

        [JsonProperty("external")]
        public string external { get; }
    }

    public class Payload
    {
        [JsonConstructor]
        public Payload(
            [JsonProperty("clock")] int clock,
            [JsonProperty("timer")] Timer timer,
            [JsonProperty("onAir")] bool onAir,
            [JsonProperty("message")] Message message,
            [JsonProperty("runtime")] Runtime runtime,
            [JsonProperty("eventNow")] EventNow eventNow,
            [JsonProperty("currentBlock")] CurrentBlock currentBlock,
            [JsonProperty("publicEventNow")] PublicEventNow publicEventNow,
            [JsonProperty("eventNext")] EventNext eventNext,
            [JsonProperty("publicEventNext")] PublicEventNext publicEventNext,
            [JsonProperty("auxtimer1")] Auxtimer1 auxtimer1,
            [JsonProperty("ping")] int ping
        )
        {
            this.clock = clock;
            this.timer = timer;
            this.onAir = onAir;
            this.message = message;
            this.runtime = runtime;
            this.eventNow = eventNow;
            this.currentBlock = currentBlock;
            this.publicEventNow = publicEventNow;
            this.eventNext = eventNext;
            this.publicEventNext = publicEventNext;
            this.auxtimer1 = auxtimer1;
            this.ping = ping;
        }

        [JsonProperty("clock")]
        public int clock { get; }

        [JsonProperty("timer")]
        public Timer timer { get; }

        [JsonProperty("onAir")]
        public bool onAir { get; }

        [JsonProperty("message")]
        public Message message { get; }

        [JsonProperty("runtime")]
        public Runtime runtime { get; }

        [JsonProperty("eventNow")]
        public EventNow eventNow { get; }

        [JsonProperty("currentBlock")]
        public CurrentBlock currentBlock { get; }

        [JsonProperty("publicEventNow")]
        public PublicEventNow publicEventNow { get; }

        [JsonProperty("eventNext")]
        public EventNext eventNext { get; }

        [JsonProperty("publicEventNext")]
        public PublicEventNext publicEventNext { get; }

        [JsonProperty("auxtimer1")]
        public Auxtimer1 auxtimer1 { get; }

        [JsonProperty("ping")]
        public int ping { get; }
    }

    public class PublicEventNext
    {
        [JsonConstructor]
        public PublicEventNext(
            [JsonProperty("type")] string type,
            [JsonProperty("id")] string id,
            [JsonProperty("cue")] string cue,
            [JsonProperty("title")] string title,
            [JsonProperty("note")] string note,
            [JsonProperty("endAction")] string endAction,
            [JsonProperty("timerType")] string timerType,
            [JsonProperty("countToEnd")] bool countToEnd,
            [JsonProperty("linkStart")] object linkStart,
            [JsonProperty("timeStrategy")] string timeStrategy,
            [JsonProperty("timeStart")] int timeStart,
            [JsonProperty("timeEnd")] int timeEnd,
            [JsonProperty("duration")] int duration,
            [JsonProperty("isPublic")] bool isPublic,
            [JsonProperty("skip")] bool skip,
            [JsonProperty("colour")] string colour,
            [JsonProperty("revision")] int revision,
            [JsonProperty("delay")] int delay,
            [JsonProperty("dayOffset")] int dayOffset,
            [JsonProperty("gap")] int gap,
            [JsonProperty("timeWarning")] int timeWarning,
            [JsonProperty("timeDanger")] int timeDanger,
            [JsonProperty("custom")] Custom custom
        )
        {
            this.type = type;
            this.id = id;
            this.cue = cue;
            this.title = title;
            this.note = note;
            this.endAction = endAction;
            this.timerType = timerType;
            this.countToEnd = countToEnd;
            this.linkStart = linkStart;
            this.timeStrategy = timeStrategy;
            this.timeStart = timeStart;
            this.timeEnd = timeEnd;
            this.duration = duration;
            this.isPublic = isPublic;
            this.skip = skip;
            this.colour = colour;
            this.revision = revision;
            this.delay = delay;
            this.dayOffset = dayOffset;
            this.gap = gap;
            this.timeWarning = timeWarning;
            this.timeDanger = timeDanger;
            this.custom = custom;
        }

        [JsonProperty("type")]
        public string type { get; }

        [JsonProperty("id")]
        public string id { get; }

        [JsonProperty("cue")]
        public string cue { get; }

        [JsonProperty("title")]
        public string title { get; }

        [JsonProperty("note")]
        public string note { get; }

        [JsonProperty("endAction")]
        public string endAction { get; }

        [JsonProperty("timerType")]
        public string timerType { get; }

        [JsonProperty("countToEnd")]
        public bool countToEnd { get; }

        [JsonProperty("linkStart")]
        public object linkStart { get; }

        [JsonProperty("timeStrategy")]
        public string timeStrategy { get; }

        [JsonProperty("timeStart")]
        public int timeStart { get; }

        [JsonProperty("timeEnd")]
        public int timeEnd { get; }

        [JsonProperty("duration")]
        public int duration { get; }

        [JsonProperty("isPublic")]
        public bool isPublic { get; }

        [JsonProperty("skip")]
        public bool skip { get; }

        [JsonProperty("colour")]
        public string colour { get; }

        [JsonProperty("revision")]
        public int revision { get; }

        [JsonProperty("delay")]
        public int delay { get; }

        [JsonProperty("dayOffset")]
        public int dayOffset { get; }

        [JsonProperty("gap")]
        public int gap { get; }

        [JsonProperty("timeWarning")]
        public int timeWarning { get; }

        [JsonProperty("timeDanger")]
        public int timeDanger { get; }

        [JsonProperty("custom")]
        public Custom custom { get; }
    }

    public class PublicEventNow
    {
        [JsonConstructor]
        public PublicEventNow(
            [JsonProperty("type")] string type,
            [JsonProperty("id")] string id,
            [JsonProperty("cue")] string cue,
            [JsonProperty("title")] string title,
            [JsonProperty("note")] string note,
            [JsonProperty("endAction")] string endAction,
            [JsonProperty("timerType")] string timerType,
            [JsonProperty("countToEnd")] bool countToEnd,
            [JsonProperty("linkStart")] object linkStart,
            [JsonProperty("timeStrategy")] string timeStrategy,
            [JsonProperty("timeStart")] int timeStart,
            [JsonProperty("timeEnd")] int timeEnd,
            [JsonProperty("duration")] int duration,
            [JsonProperty("isPublic")] bool isPublic,
            [JsonProperty("skip")] bool skip,
            [JsonProperty("colour")] string colour,
            [JsonProperty("revision")] int revision,
            [JsonProperty("delay")] int delay,
            [JsonProperty("dayOffset")] int dayOffset,
            [JsonProperty("gap")] int gap,
            [JsonProperty("timeWarning")] int timeWarning,
            [JsonProperty("timeDanger")] int timeDanger,
            [JsonProperty("custom")] Custom custom
        )
        {
            this.type = type;
            this.id = id;
            this.cue = cue;
            this.title = title;
            this.note = note;
            this.endAction = endAction;
            this.timerType = timerType;
            this.countToEnd = countToEnd;
            this.linkStart = linkStart;
            this.timeStrategy = timeStrategy;
            this.timeStart = timeStart;
            this.timeEnd = timeEnd;
            this.duration = duration;
            this.isPublic = isPublic;
            this.skip = skip;
            this.colour = colour;
            this.revision = revision;
            this.delay = delay;
            this.dayOffset = dayOffset;
            this.gap = gap;
            this.timeWarning = timeWarning;
            this.timeDanger = timeDanger;
            this.custom = custom;
        }

        [JsonProperty("type")]
        public string type { get; }

        [JsonProperty("id")]
        public string id { get; }

        [JsonProperty("cue")]
        public string cue { get; }

        [JsonProperty("title")]
        public string title { get; }

        [JsonProperty("note")]
        public string note { get; }

        [JsonProperty("endAction")]
        public string endAction { get; }

        [JsonProperty("timerType")]
        public string timerType { get; }

        [JsonProperty("countToEnd")]
        public bool countToEnd { get; }

        [JsonProperty("linkStart")]
        public object linkStart { get; }

        [JsonProperty("timeStrategy")]
        public string timeStrategy { get; }

        [JsonProperty("timeStart")]
        public int timeStart { get; }

        [JsonProperty("timeEnd")]
        public int timeEnd { get; }

        [JsonProperty("duration")]
        public int duration { get; }

        [JsonProperty("isPublic")]
        public bool isPublic { get; }

        [JsonProperty("skip")]
        public bool skip { get; }

        [JsonProperty("colour")]
        public string colour { get; }

        [JsonProperty("revision")]
        public int revision { get; }

        [JsonProperty("delay")]
        public int delay { get; }

        [JsonProperty("dayOffset")]
        public int dayOffset { get; }

        [JsonProperty("gap")]
        public int gap { get; }

        [JsonProperty("timeWarning")]
        public int timeWarning { get; }

        [JsonProperty("timeDanger")]
        public int timeDanger { get; }

        [JsonProperty("custom")]
        public Custom custom { get; }
    }

    public class Root
    {
        [JsonConstructor]
        public Root(
            [JsonProperty("payload")] Payload payload
        )
        {
            this.payload = payload;
        }

        [JsonProperty("payload")]
        public Payload payload { get; }
    }

    public class Runtime
    {
        [JsonConstructor]
        public Runtime(
            [JsonProperty("selectedEventIndex")] int selectedEventIndex,
            [JsonProperty("numEvents")] int numEvents,
            [JsonProperty("offset")] int offset,
            [JsonProperty("relativeOffset")] int relativeOffset,
            [JsonProperty("plannedStart")] int plannedStart,
            [JsonProperty("plannedEnd")] int plannedEnd,
            [JsonProperty("actualStart")] int actualStart,
            [JsonProperty("expectedEnd")] int expectedEnd,
            [JsonProperty("offsetMode")] string offsetMode
        )
        {
            this.selectedEventIndex = selectedEventIndex;
            this.numEvents = numEvents;
            this.offset = offset;
            this.relativeOffset = relativeOffset;
            this.plannedStart = plannedStart;
            this.plannedEnd = plannedEnd;
            this.actualStart = actualStart;
            this.expectedEnd = expectedEnd;
            this.offsetMode = offsetMode;
        }

        [JsonProperty("selectedEventIndex")]
        public int selectedEventIndex { get; }

        [JsonProperty("numEvents")]
        public int numEvents { get; }

        [JsonProperty("offset")]
        public int offset { get; }

        [JsonProperty("relativeOffset")]
        public int relativeOffset { get; }

        [JsonProperty("plannedStart")]
        public int plannedStart { get; }

        [JsonProperty("plannedEnd")]
        public int plannedEnd { get; }

        [JsonProperty("actualStart")]
        public int actualStart { get; }

        [JsonProperty("expectedEnd")]
        public int expectedEnd { get; }

        [JsonProperty("offsetMode")]
        public string offsetMode { get; }
    }

    public class Timer
    {
        [JsonConstructor]
        public Timer(
            [JsonProperty("addedTime")] int addedTime,
            [JsonProperty("current")] int current,
            [JsonProperty("duration")] int duration,
            [JsonProperty("elapsed")] int elapsed,
            [JsonProperty("expectedFinish")] int expectedFinish,
            [JsonProperty("finishedAt")] object finishedAt,
            [JsonProperty("phase")] string phase,
            [JsonProperty("playback")] string playback,
            [JsonProperty("secondaryTimer")] object secondaryTimer,
            [JsonProperty("startedAt")] int startedAt,
            [JsonProperty("text")] string text,
            [JsonProperty("visible")] bool visible,
            [JsonProperty("blink")] bool blink,
            [JsonProperty("blackout")] bool blackout,
            [JsonProperty("secondarySource")] object secondarySource
        )
        {
            this.addedTime = addedTime;
            this.current = current;
            this.duration = duration;
            this.elapsed = elapsed;
            this.expectedFinish = expectedFinish;
            this.finishedAt = finishedAt;
            this.phase = phase;
            this.playback = playback;
            this.secondaryTimer = secondaryTimer;
            this.startedAt = startedAt;
            this.text = text;
            this.visible = visible;
            this.blink = blink;
            this.blackout = blackout;
            this.secondarySource = secondarySource;
        }

        [JsonProperty("addedTime")]
        public int addedTime { get; }

        [JsonProperty("current")]
        public int current { get; }

        [JsonProperty("duration")]
        public int duration { get; }

        [JsonProperty("elapsed")]
        public int elapsed { get; }

        [JsonProperty("expectedFinish")]
        public int expectedFinish { get; }

        [JsonProperty("finishedAt")]
        public object finishedAt { get; }

        [JsonProperty("phase")]
        public string phase { get; }

        [JsonProperty("playback")]
        public string playback { get; }

        [JsonProperty("secondaryTimer")]
        public object secondaryTimer { get; }

        [JsonProperty("startedAt")]
        public int startedAt { get; }

        [JsonProperty("text")]
        public string text { get; }

        [JsonProperty("visible")]
        public bool visible { get; }

        [JsonProperty("blink")]
        public bool blink { get; }

        [JsonProperty("blackout")]
        public bool blackout { get; }

        [JsonProperty("secondarySource")]
        public object secondarySource { get; }
    }

}
