﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace dotIRC.Ctcp
{
    // Defines all message processors for the client.
    partial class CtcpClient
    {
        /// <summary>
        /// Process ACTION messages received from a user.
        /// </summary>
        /// <param name="message">The message received from the user.</param>
        [MessageProcessor("action")]
        protected void ProcessMessageAction(CtcpMessage message)
        {
            Debug.Assert(message.Data != null);

            if (!message.IsResponse)
            {
                var text = message.Data;

                OnActionReceived(new CtcpMessageEventArgs(message.Source, message.Targets, text));
            }
        }

        /// <summary>
        /// Process TIME messages received from a user.
        /// </summary>
        /// <param name="message">The message received from the user.</param>
        [MessageProcessor("time")]
        protected void ProcessMessageTime(CtcpMessage message)
        {
            if (message.IsResponse)
            {
                var dateTime = message.Data;

                OnTimeResponseReceived(new CtcpTimeResponseReceivedEventArgs(message.Source, dateTime));
            }
            else
            {
                var localDateTime = DateTimeOffset.Now.ToString("o");

                SendMessageTime(new[] { message.Source }, localDateTime, true);
            }
        }

        /// <summary>
        /// Process VERSION messages received from a user.
        /// </summary>
        /// <param name="message">The message received from the user.</param>
        [MessageProcessor("version")]
        protected void ProcessMessageVersion(CtcpMessage message)
        {
            if (message.IsResponse)
            {
                var versionInfo = message.Data;

                OnVersionResponseReceived(new CtcpVersionResponseReceivedEventArgs(message.Source, versionInfo));
            }
            else
            {
                if (this.ClientVersion != null)
                {
                    SendMessageVersion(new[] { message.Source }, this.ClientVersion, true);
                }
            }
        }

        /// <summary>
        /// Process ERRMSG messages received from a user.
        /// </summary>
        /// <param name="message">The message received from the user.</param>
        [MessageProcessor("errmsg")]
        protected void ProcessMessageErrMsg(CtcpMessage message)
        {
            Debug.Assert(message.Data != null);

            if (message.IsResponse)
            {
                // Get failed query and error message from data.
                var parts = message.Data.SplitIntoPair(" :");
                var failedQuery = parts.Item1;
                var errorMessage = parts.Item2;

                OnErrorMessageResponseReceived(new CtcpErrorMessageReceivedEventArgs(message.Source,
                    failedQuery, errorMessage));
            }
            else
            {
                SendMessageErrMsg(new[] { message.Source }, message.Data + " :" + messageNoError, true);
            }
        }

        /// <summary>
        /// Process PING messages received from a user.
        /// </summary>
        /// <param name="message">The message received from the user.</param>
        [MessageProcessor("ping")]
        protected void ProcessMessagePing(CtcpMessage message)
        {
            Debug.Assert(message.Data != null);

            if (message.IsResponse)
            {
                // Calculate time elapsed since the ping request was sent.
                var sendTime = new DateTime(long.Parse(message.Data));
                var pingTime = DateTime.Now - sendTime;

                OnPingResponseReceived(new CtcpPingResponseReceivedEventArgs(message.Source, pingTime));
            }
            else
            {
                SendMessagePing(new[] { message.Source }, message.Data, true);
            }
        }
    }
}
