import AppKit

@MainActor final class ToastPlugin {
    private static var instance: ToastPlugin?
    static func register() {
        if instance == nil {
            instance = ToastPlugin()
        }
    }

    private init() {
        let channel = NativeChannels.channel("dotnative.toast")
        channel.handle("show") {
            [weak self] arguments, reply in
            self?.show(arguments, reply)
                ?? reply.failure("unavailable", "Toast plugin is unavailable")
        }
    }

    private func show(_ arguments: PluginValue, _ reply: PluginReply) {
        let fields = arguments.fields
        guard let message = fields["message"]?.string, !message.isEmpty, message.count <= 2_000,
            let view = NativeChannels.presenter?.view
        else {
            reply.failure("invalid_argument", "A message and an active window are required")
            return
        }
        let label = NSTextField(labelWithString: message)
        label.font = .systemFont(ofSize: 16)
        label.textColor = NSColor(white: 0.5, alpha: 1)
        label.maximumNumberOfLines = 8
        label.lineBreakMode = .byTruncatingTail
        let measured = label.sizeThatFits(
            NSSize(width: view.bounds.width * 0.8, height: view.bounds.height * 0.8))
        label.frame = NSRect(x: 10, y: 10, width: measured.width, height: measured.height)
        let toast = NSView(
            frame: NSRect(x: 0, y: 0, width: measured.width + 20, height: measured.height + 20))
        toast.wantsLayer = true
        toast.layer?.backgroundColor = NSColor(white: 0.96, alpha: 1).cgColor
        toast.layer?.cornerRadius = 10
        toast.addSubview(label)
        toast.frame.origin = NSPoint(
            x: view.bounds.midX - toast.frame.width / 2,
            y: view.bounds.midY - toast.frame.height / 2
        )
        view.addSubview(toast)
        reply.success()
        DispatchQueue.main.asyncAfter(deadline: .now() + 2) {
            toast.removeFromSuperview()
        }
    }
}
