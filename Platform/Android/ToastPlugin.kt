package com.dotnative.plugins

import android.app.Activity
import android.graphics.Color
import android.graphics.drawable.GradientDrawable
import android.view.Gravity
import android.widget.TextView
import android.widget.Toast

class ToastPlugin(private val activity: Activity) {
    init {
        val channel = NativeChannels.channel("dotnative.toast")
        channel.handle("show") { args, reply ->
            val message = (args as? Map<*, *>)?.get("message") as? String
            if (message.isNullOrBlank() || message.length > 2_000) {
                reply.failure("invalid_argument", "A message of at most 2000 characters is required")
                return@handle
            }
            activity.runOnUiThread {
                show(message)
                reply.success()
            }
        }
    }

    @Suppress("DEPRECATION")
    private fun show(message: String) {
        val density = activity.resources.displayMetrics.density
        val background =
            GradientDrawable().apply {
                setColor(Color.rgb(246, 246, 246))
                cornerRadius = 40 * density
            }
        val text =
            TextView(activity).apply {
                this.text = message
                textSize = 16f
                setTextColor(Color.rgb(128, 128, 128))
                gravity = Gravity.CENTER
                maxLines = 8
                setPadding(
                    (16 * density).toInt(),
                    (10 * density).toInt(),
                    (16 * density).toInt(),
                    (10 * density).toInt(),
                )
                this.background = background
            }
        Toast(activity).apply {
            duration = Toast.LENGTH_SHORT
            view = text
            setGravity(Gravity.CENTER, 0, 0)
            show()
        }
    }
}
